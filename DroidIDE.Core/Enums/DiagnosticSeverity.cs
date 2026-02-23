namespace DroidIDE.Core.Enums;

/// <summary>
/// Severity level of a code diagnostic from the compiler or Roslyn analyzers.
/// Ordered from lowest to highest severity.
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>A suppressed or hidden diagnostic that is not displayed to the user.</summary>
    Hidden,

    /// <summary>An informational diagnostic providing hints or suggestions.</summary>
    Info,

    /// <summary>A warning diagnostic indicating a potential issue that does not prevent compilation.</summary>
    Warning,

    /// <summary>An error diagnostic indicating a compilation failure.</summary>
    Error
}
