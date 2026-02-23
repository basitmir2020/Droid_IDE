using DroidIDE.Core.Enums;
using DroidIDE.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace DroidIDE.Editor.Diagnostics;

/// <summary>
/// Provides real-time error and warning analysis for C# source code using Roslyn.
/// Compiles single files against core BCL references and extracts diagnostics
/// as <see cref="DiagnosticItem"/> objects for display in the editor.
/// </summary>
public class DiagnosticService
{
    /// <summary>
    /// Core BCL metadata references for standalone compilation analysis.
    /// </summary>
    private static readonly MetadataReference[] DefaultReferences =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
    ];

    /// <summary>
    /// Raised when diagnostics are updated for a file.
    /// The first parameter is the file path; the second is the list of diagnostics.
    /// </summary>
    public event Action<string, List<DiagnosticItem>>? DiagnosticsUpdated;

    /// <summary>
    /// Analyzes a C# source file by compiling it and extracting compiler diagnostics.
    /// </summary>
    /// <param name="filePath">The absolute file path (used for diagnostic location reporting).</param>
    /// <param name="content">The C# source code content to analyze.</param>
    /// <returns>
    /// A list of <see cref="DiagnosticItem"/> objects representing errors, warnings, and info messages.
    /// Hidden-severity diagnostics are excluded.
    /// </returns>
    /// <remarks>
    /// Runs the analysis on a background thread via <see cref="Task.Run"/> to avoid blocking the UI.
    /// Fires the <see cref="DiagnosticsUpdated"/> event when analysis completes.
    /// </remarks>
    public async Task<List<DiagnosticItem>> AnalyzeAsync(string filePath, string content)
    {
        return await Task.Run(() =>
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                content,
                new CSharpParseOptions(LanguageVersion.Latest),
                filePath);

            var compilation = CSharpCompilation.Create(
                "DiagnosticAnalysis",
                [syntaxTree],
                DefaultReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var roslynDiagnostics = compilation.GetDiagnostics();
            var items = new List<DiagnosticItem>();

            foreach (var diag in roslynDiagnostics)
            {
                // Skip hidden diagnostics
                if (diag.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Hidden)
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
        });
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
