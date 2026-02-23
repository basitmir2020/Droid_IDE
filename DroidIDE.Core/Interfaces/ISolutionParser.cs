using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Parses .sln and .slnx solution files to extract project references and solution metadata.
/// Implemented by <c>SolutionParser</c> in the ProjectSystem layer.
/// </summary>
public interface ISolutionParser
{
    /// <summary>Parses a solution file and returns the solution metadata with its constituent projects.</summary>
    /// <param name="solutionPath">The absolute path to the .sln or .slnx file.</param>
    /// <returns>A <see cref="SolutionInfo"/> containing the solution name, path, and project list.</returns>
    Task<SolutionInfo> ParseAsync(string solutionPath);

    /// <summary>Determines whether a file is a solution file based on its extension (.sln or .slnx).</summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><c>true</c> if the file has a .sln or .slnx extension; otherwise, <c>false</c>.</returns>
    bool IsSolutionFile(string filePath);
}
