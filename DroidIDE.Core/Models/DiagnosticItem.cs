using DroidIDE.Core.Enums;

namespace DroidIDE.Core.Models;

/// <summary>
/// Represents a code diagnostic (error, warning, info) from the compiler or Roslyn.
/// </summary>
public class DiagnosticItem
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
    public DiagnosticSeverity Severity { get; set; } = DiagnosticSeverity.Info;
    public string Source { get; set; } = "compiler";

    public string DisplayLocation => $"{Path.GetFileName(FilePath)}({Line},{Column})";
}
