using System.Text.RegularExpressions;

namespace DroidIDE.Terminal.OutputParser;

/// <summary>
/// Strips ANSI escape codes from terminal output for clean plain-text display in the IDE.
/// Uses source-generated regex via <see cref="GeneratedRegexAttribute"/> for compile-time optimization.
/// </summary>
public static partial class AnsiParser
{
    /// <summary>
    /// Source-generated regex matching ANSI escape sequences including CSI, OSC, and other control sequences.
    /// </summary>
    [GeneratedRegex(@"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~]|\].*?(?:\x07|\x1B\\))")]
    private static partial Regex AnsiEscapeRegex();

    /// <summary>
    /// Removes all ANSI escape sequences from a single string.
    /// </summary>
    /// <param name="input">The raw terminal output string potentially containing ANSI codes.</param>
    /// <returns>The input string with all ANSI escape sequences removed. Returns the original string if null or empty.</returns>
    public static string StripAnsi(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return AnsiEscapeRegex().Replace(input, string.Empty);
    }

    /// <summary>
    /// Strips ANSI escape codes from each line in a collection of output lines.
    /// </summary>
    /// <param name="lines">A collection of raw terminal output lines.</param>
    /// <returns>An enumerable of cleaned lines with ANSI codes removed.</returns>
    public static IEnumerable<string> StripAnsiLines(IEnumerable<string> lines)
    {
        return lines.Select(StripAnsi);
    }
}
