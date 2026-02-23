using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Parses .sln and .slnx solution files to extract project references.
/// </summary>
public interface ISolutionParser
{
    /// <summary>Parse a solution file and return solution metadata with project list.</summary>
    Task<SolutionInfo> ParseAsync(string solutionPath);

    /// <summary>Detect if a file is a solution file (.sln or .slnx).</summary>
    bool IsSolutionFile(string filePath);
}
