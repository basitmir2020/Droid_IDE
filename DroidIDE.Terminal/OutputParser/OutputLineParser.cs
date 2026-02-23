using System.Text.RegularExpressions;
using DroidIDE.Core.Enums;
using DroidIDE.Core.Models;

namespace DroidIDE.Terminal.OutputParser;

/// <summary>
/// Parses compiler/build output lines into structured diagnostic items.
/// Handles MSBuild-style error/warning messages.
/// </summary>
public static partial class OutputLineParser
{
    // Matches MSBuild diagnostic format: filepath(line,col): error/warning CODE: message
    [GeneratedRegex(@"^(.+?)\((\d+),(\d+)\):\s+(error|warning)\s+(\w+):\s+(.+)$")]
    private static partial Regex MsBuildDiagnosticRegex();

    /// <summary>
    /// Attempts to parse a build output line into a DiagnosticItem.
    /// Returns null if the line is not a diagnostic.
    /// </summary>
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
    /// Parses multiple output lines, returning only those that are diagnostics.
    /// </summary>
    public static List<DiagnosticItem> ParseDiagnostics(IEnumerable<string> lines)
    {
        return lines
            .Select(TryParseDiagnostic)
            .Where(d => d is not null)
            .Cast<DiagnosticItem>()
            .ToList();
    }
}
