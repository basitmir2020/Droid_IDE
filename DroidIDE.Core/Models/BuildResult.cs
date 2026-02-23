using DroidIDE.Core.Enums;

namespace DroidIDE.Core.Models;

/// <summary>
/// Represents the result of a dotnet build operation.
/// </summary>
public class BuildResult
{
    public bool Success { get; set; }
    public BuildStatus Status { get; set; } = BuildStatus.Idle;
    public List<DiagnosticItem> Diagnostics { get; set; } = [];
    public List<string> OutputLines { get; set; } = [];
    public TimeSpan Duration { get; set; }

    public int ErrorCount => Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
    public int WarningCount => Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);
}
