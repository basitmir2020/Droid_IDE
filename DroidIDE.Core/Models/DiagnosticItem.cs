using DroidIDE.Core.Enums;

namespace DroidIDE.Core.Models;

/// <summary>
/// Represents a code diagnostic (error, warning, info, or hint) produced by the compiler or Roslyn analyzers.
/// Maps to MSBuild output format: <c>File(Line,Column): severity Code: Message</c>.
/// </summary>
public class DiagnosticItem
{
    /// <summary>Gets or sets the diagnostic code (e.g., "CS0173", "IDE0001").</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable diagnostic message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the absolute path to the source file containing the diagnostic.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the 1-based start line number of the diagnostic location.</summary>
    public int Line { get; set; }

    /// <summary>Gets or sets the 1-based start column number of the diagnostic location.</summary>
    public int Column { get; set; }

    /// <summary>Gets or sets the 1-based end line number of the diagnostic span.</summary>
    public int EndLine { get; set; }

    /// <summary>Gets or sets the 1-based end column number of the diagnostic span.</summary>
    public int EndColumn { get; set; }

    /// <summary>Gets or sets the severity level of this diagnostic.</summary>
    public DiagnosticSeverity Severity { get; set; } = DiagnosticSeverity.Info;

    /// <summary>Gets or sets the source of the diagnostic (e.g., "compiler", "roslyn", "analyzer").</summary>
    public string Source { get; set; } = "compiler";

    /// <summary>Gets a formatted display string showing the file name and location (e.g., "Program.cs(12,5)").</summary>
    public string DisplayLocation => $"{Path.GetFileName(FilePath)}({Line},{Column})";
}
