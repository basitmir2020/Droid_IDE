namespace DroidIDE.Core.Models;

/// <summary>
/// Represents a .NET solution parsed from a .sln or .slnx file.
/// Contains the solution metadata and the list of constituent projects.
/// </summary>
public class SolutionInfo
{
    /// <summary>Gets or sets the solution name (derived from the .sln filename).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the absolute path to the .sln or .slnx file.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the directory containing the solution file.</summary>
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the list of projects belonging to this solution.</summary>
    public List<ProjectInfo> Projects { get; set; } = [];

    /// <summary>
    /// Gets the startup project — the first executable project in the solution,
    /// or the first project if none are executable. Returns <c>null</c> if the solution has no projects.
    /// </summary>
    public ProjectInfo? StartupProject => 
        Projects.FirstOrDefault(p => p.IsExecutable) ?? Projects.FirstOrDefault();
}
