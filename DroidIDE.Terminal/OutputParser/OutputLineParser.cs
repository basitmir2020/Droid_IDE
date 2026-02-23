using System.Text.RegularExpressions;
using DroidIDE.Core.Enums;
using DroidIDE.Core.Models;

namespace DroidIDE.Terminal.OutputParser;

/// <summary>
/// Parses compiler and build output lines into structured <see cref="DiagnosticItem"/> objects.
/// Handles the MSBuild diagnostic format: <c>filepath(line,col): error/warning CODE: message</c>.
/// Uses source-generated regex via <see cref="GeneratedRegexAttribute"/> for compile-time optimization.
/// </summary>
public static partial class OutputLineParser
{
    /// <summary>
    /// Source-generated regex matching the MSBuild diagnostic output format.
    /// Captures: filepath, line, column, severity (error/warning), code, and message.
    /// </summary>
    [GeneratedRegex(@"^(.+?)\((\d+),(\d+)\):\s+(error|warning)\s+(\w+):\s+(.+)$")]
    private static partial Regex MsBuildDiagnosticRegex();

    /// <summary>
    /// Attempts to parse a single build output line into a structured <see cref="DiagnosticItem"/>.
    /// </summary>
    /// <param name="line">A raw build output line from MSBuild or the dotnet CLI.</param>
    /// <returns>
    /// A <see cref="DiagnosticItem"/> if the line matches the MSBuild diagnostic format;
    /// otherwise, <c>null</c> for non-diagnostic lines.
    /// </returns>
    /// <remarks>
    /// ANSI escape codes are automatically stripped before pattern matching via <see cref="AnsiParser.StripAnsi"/>.
    /// </remarks>
    public static DiagnosticItem? TryParseDiagnostic(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        var cleanLine = AnsiParser.StripAnsi(line);
        var match = MsBuildDiagnosticRegex().Match(cleanLine);

        if (!match.Success)
            return null;

        return new DiagnosticItem
        {
            FilePath = match.Groups[1].Value.Trim(),
            Line = int.Parse(match.Groups[2].Value),
            Column = int.Parse(match.Groups[3].Value),
            Severity = match.Groups[4].Value == "error"
                ? DiagnosticSeverity.Error
                : DiagnosticSeverity.Warning,
            Id = match.Groups[5].Value,
            Message = match.Groups[6].Value,
            Source = "msbuild"
        };
    }

    /// <summary>
    /// Parses multiple build output lines and returns only those that are valid diagnostics.
    /// </summary>
    /// <param name="lines">A collection of raw build output lines.</param>
    /// <returns>A list of <see cref="DiagnosticItem"/> objects parsed from the diagnostic lines.</returns>
    public static List<DiagnosticItem> ParseDiagnostics(IEnumerable<string> lines)
    {
        return lines
            .Select(TryParseDiagnostic)
            .Where(d => d is not null)
            .Cast<DiagnosticItem>()
            .ToList();
    }
}
