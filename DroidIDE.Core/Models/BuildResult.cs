using DroidIDE.Core.Enums;

namespace DroidIDE.Core.Models;

/// <summary>
/// Represents the result of a <c>dotnet build</c> operation, including success status,
/// compiler diagnostics, raw output, and elapsed time.
/// </summary>
public class BuildResult
{
    /// <summary>Gets or sets a value indicating whether the build completed successfully (exit code 0).</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the current build lifecycle status.</summary>
    public BuildStatus Status { get; set; } = BuildStatus.Idle;

    /// <summary>Gets or sets the list of compiler diagnostics (errors, warnings, info) extracted from build output.</summary>
    public List<DiagnosticItem> Diagnostics { get; set; } = [];

    /// <summary>Gets or sets the raw MSBuild output lines from the build process.</summary>
    public List<string> OutputLines { get; set; } = [];

    /// <summary>Gets or sets the total elapsed time for the build operation.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Gets the number of error-severity diagnostics in the build result.</summary>
    public int ErrorCount => Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>Gets the number of warning-severity diagnostics in the build result.</summary>
    public int WarningCount => Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);
}
