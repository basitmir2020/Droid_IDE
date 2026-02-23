namespace DroidIDE.Core.Models;

/// <summary>
/// Represents a .NET solution parsed from a .sln or .slnx file.
/// </summary>
public class SolutionInfo
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string DirectoryPath { get; set; } = string.Empty;
    public List<ProjectInfo> Projects { get; set; } = [];

    /// <summary>
    /// The startup project (first executable project, or first project).
    /// </summary>
    public ProjectInfo? StartupProject => 
        Projects.FirstOrDefault(p => p.IsExecutable) ?? Projects.FirstOrDefault();
}
