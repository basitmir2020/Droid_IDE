using System.Text.RegularExpressions;

namespace DroidIDE.Terminal.OutputParser;

/// <summary>
/// Strips ANSI escape codes from terminal output for plain-text display.
/// </summary>
public static partial class AnsiParser
{
    // Regex matching ANSI escape sequences (CSI sequences, OSC, etc.)
    [GeneratedRegex(@"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~]|\].*?(?:\x07|\x1B\\))")]
    private static partial Regex AnsiEscapeRegex();

    /// <summary>
    /// Removes all ANSI escape sequences from a string.
    /// </summary>
    public static string StripAnsi(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return AnsiEscapeRegex().Replace(input, string.Empty);
    }

    /// <summary>
    /// Strips ANSI codes from each line in a collection.
    /// </summary>
    public static IEnumerable<string> StripAnsiLines(IEnumerable<string> lines)
    {
        return lines.Select(StripAnsi);
    }
}
