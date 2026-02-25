using DroidIDE.Core.Enums;
using DroidIDE.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using DroidIDE.Editor.LanguageServer;

namespace DroidIDE.Editor.Diagnostics;

/// <summary>
/// Provides real-time error and warning analysis for C# source code using Roslyn.
/// Compiles single files against core BCL references and extracts diagnostics
/// as <see cref="DiagnosticItem"/> objects for display in the editor.
/// </summary>
public class DiagnosticService
{
    private readonly RoslynLanguageService _languageService;

    /// <summary>
    /// Initializes a new instance of <see cref="DiagnosticService"/> using the shared Roslyn language service.
    /// </summary>
    public DiagnosticService(RoslynLanguageService languageService)
    {
        _languageService = languageService;
    }

    /// <summary>
    /// Raised when diagnostics are updated for a file.
    /// The first parameter is the file path; the second is the list of diagnostics.
    /// </summary>
    public event Action<string, List<DiagnosticItem>>? DiagnosticsUpdated;

    /// <summary>
    /// Analyzes a C# source file. Leverages the shared workspace in <see cref="RoslynLanguageService"/> 
    /// for performance, with a standalone fallback if the file isn't in the workspace.
    /// </summary>
    /// <param name="filePath">The absolute file path (used for diagnostic location reporting).</param>
    /// <param name="content">The C# source code content to analyze.</param>
    /// <param name="cancellationToken">Token used to cancel analysis when content changes again.</param>
    /// <returns>A list of <see cref="DiagnosticItem"/> objects.</returns>
    public async Task<List<DiagnosticItem>> AnalyzeAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        // First, try to get diagnostics from the active workspace (much faster)
        var roslynDiagnostics = await _languageService.GetDiagnosticsAsync(filePath, cancellationToken);
        
        // If the workspace didn't provide any (e.g. document not added yet), perform a standalone parse
        if (roslynDiagnostics.IsEmpty)
        {
            roslynDiagnostics = await Task.Run(() =>
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    content,
                    new CSharpParseOptions(LanguageVersion.Latest),
                    filePath,
                    cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested) return [];

                var compilation = CSharpCompilation.Create(
                    "StandaloneDiagnosticAnalysis",
                    [syntaxTree],
                    _languageService.References,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                return compilation.GetDiagnostics(cancellationToken);
            }, cancellationToken);
        }

        if (cancellationToken.IsCancellationRequested) return [];

        var items = new List<DiagnosticItem>();
        foreach (var diag in roslynDiagnostics)
        {
            if (cancellationToken.IsCancellationRequested) break;

            // Skip hidden diagnostics
            if (diag.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Hidden)
                continue;

            // Filter out diagnostics from other files in the same compilation
            if (diag.Location.SourceTree != null && diag.Location.SourceTree.FilePath != filePath)
                continue;

            var location = diag.Location;
            var lineSpan = location.GetLineSpan();

            items.Add(new DiagnosticItem
            {
                Id = diag.Id,
                Message = diag.GetMessage(),
                FilePath = filePath,
                Line = lineSpan.StartLinePosition.Line + 1,
                Column = lineSpan.StartLinePosition.Character + 1,
                EndLine = lineSpan.EndLinePosition.Line + 1,
                EndColumn = lineSpan.EndLinePosition.Character + 1,
                Severity = MapSeverity(diag.Severity),
                Source = "roslyn"
            });
        }

        DiagnosticsUpdated?.Invoke(filePath, items);
        return items;
    }

    /// <summary>
    /// Maps Roslyn's <see cref="Microsoft.CodeAnalysis.DiagnosticSeverity"/> to the
    /// DroidIDE Core <see cref="DroidIDE.Core.Enums.DiagnosticSeverity"/> enum.
    /// </summary>
    /// <param name="severity">The Roslyn diagnostic severity to convert.</param>
    /// <returns>The corresponding Core enum value.</returns>
    private static DroidIDE.Core.Enums.DiagnosticSeverity MapSeverity(
        Microsoft.CodeAnalysis.DiagnosticSeverity severity)
    {
        return severity switch
        {
            Microsoft.CodeAnalysis.DiagnosticSeverity.Error => Core.Enums.DiagnosticSeverity.Error,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning => Core.Enums.DiagnosticSeverity.Warning,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Info => Core.Enums.DiagnosticSeverity.Info,
            _ => Core.Enums.DiagnosticSeverity.Hidden
        };
    }
}
