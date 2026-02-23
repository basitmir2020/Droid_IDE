using DroidIDE.Core.Enums;
using DroidIDE.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace DroidIDE.Editor.Diagnostics;

/// <summary>
/// Provides real-time error and warning analysis for C# source code.
/// Compiles single files and extracts diagnostics as DiagnosticItem objects.
/// </summary>
public class DiagnosticService
{
    // Core BCL references for compilation
    private static readonly MetadataReference[] DefaultReferences =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
    ];

    /// <summary>Fired when diagnostics are updated for a file.</summary>
    public event Action<string, List<DiagnosticItem>>? DiagnosticsUpdated;

    /// <summary>
    /// Analyze a C# source file and return diagnostics.
    /// </summary>
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
    /// Convert Roslyn DiagnosticSeverity to our Core enum.
    /// </summary>
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
