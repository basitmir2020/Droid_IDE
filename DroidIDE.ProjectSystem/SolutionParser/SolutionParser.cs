using System.Text.RegularExpressions;
using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.ProjectSystem.SolutionParser;

/// <summary>
/// Implements <see cref="ISolutionParser"/> by parsing .sln and .slnx solution files
/// to extract project references. Uses source-generated regex for both the classic
/// <c>.sln</c> format and the XML-based <c>.slnx</c> format.
/// </summary>
public partial class SolutionParser : ISolutionParser
{
    private readonly IProjectParser _projectParser;

    /// <summary>
    /// Initializes a new instance of <see cref="SolutionParser"/>.
    /// </summary>
    /// <param name="projectParser">The project parser used to parse each referenced .csproj file.</param>
    public SolutionParser(IProjectParser projectParser)
    {
        _projectParser = projectParser;
    }

    /// <summary>
    /// Parses a solution file and returns solution metadata with its list of projects.
    /// Supports both classic <c>.sln</c> format and XML-based <c>.slnx</c> format.
    /// </summary>
    /// <param name="solutionPath">The absolute path to the .sln or .slnx file.</param>
    /// <returns>A <see cref="SolutionInfo"/> containing the solution metadata and parsed projects.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the solution file does not exist.</exception>
    /// <remarks>
    /// Unparseable projects are silently skipped to avoid crashing the entire solution load.
    /// Only <c>.csproj</c> files are parsed; other project types are ignored.
    /// </remarks>
    public async Task<SolutionInfo> ParseAsync(string solutionPath)
    {
        if (!File.Exists(solutionPath))
            throw new FileNotFoundException("Solution file not found.", solutionPath);

        var extension = Path.GetExtension(solutionPath).ToLowerInvariant();
        var projectPaths = extension switch
        {
            ".slnx" => await ParseSlnxAsync(solutionPath),
            _ => await ParseSlnAsync(solutionPath)
        };

        var solution = new SolutionInfo
        {
            Name = Path.GetFileNameWithoutExtension(solutionPath),
            FilePath = solutionPath,
            DirectoryPath = Path.GetDirectoryName(solutionPath) ?? string.Empty
        };

        // Parse each referenced project
        foreach (var relativeProjectPath in projectPaths)
        {
            var absolutePath = Path.GetFullPath(
                Path.Combine(solution.DirectoryPath, relativeProjectPath));

            if (File.Exists(absolutePath) && absolutePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var project = await _projectParser.ParseAsync(absolutePath);
                    solution.Projects.Add(project);
                }
                catch
                {
                    // Skip unparseable projects, don't crash the whole solution
                }
            }
        }

        return solution;
    }

    /// <summary>
    /// Determines whether a file is a solution file based on its extension.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><c>true</c> if the file has a <c>.sln</c> or <c>.slnx</c> extension; otherwise, <c>false</c>.</returns>
    public bool IsSolutionFile(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".sln" or ".slnx";
    }

    /// <summary>
    /// Parses a classic <c>.sln</c> file using regex to extract <c>Project()</c> declarations.
    /// </summary>
    /// <param name="solutionPath">The absolute path to the .sln file.</param>
    /// <returns>A list of relative project file paths referenced in the solution.</returns>
    private async Task<List<string>> ParseSlnAsync(string solutionPath)
    {
        var content = await File.ReadAllTextAsync(solutionPath);
        var projectPaths = new List<string>();

        // Match: Project("{GUID}") = "Name", "relative\path\to.csproj", "{GUID}"
        foreach (Match match in SlnProjectRegex().Matches(content))
        {
            var projectPath = match.Groups[1].Value;
            // Skip solution folders (they don't have actual project files)
            if (!string.IsNullOrWhiteSpace(projectPath))
            {
                projectPaths.Add(projectPath.Replace('\\', Path.DirectorySeparatorChar));
            }
        }

        return projectPaths;
    }

    /// <summary>
    /// Parses a <c>.slnx</c> XML-format solution file using regex to extract <c>&lt;Project&gt;</c> elements.
    /// </summary>
    /// <param name="solutionPath">The absolute path to the .slnx file.</param>
    /// <returns>A list of relative project file paths referenced in the solution.</returns>
    private async Task<List<string>> ParseSlnxAsync(string solutionPath)
    {
        var content = await File.ReadAllTextAsync(solutionPath);
        var projectPaths = new List<string>();

        // Match: <Project Path="relative\path\to.csproj" />
        foreach (Match match in SlnxProjectRegex().Matches(content))
        {
            var projectPath = match.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(projectPath))
            {
                projectPaths.Add(projectPath.Replace('\\', Path.DirectorySeparatorChar));
            }
        }

        return projectPaths;
    }

    /// <summary>
    /// Source-generated regex matching classic .sln Project() declarations.
    /// Captures the relative project path from: <c>Project("{GUID}") = "Name", "Path\To\Project.csproj", "{GUID}"</c>.
    /// </summary>
    [GeneratedRegex(@"Project\(""\{[^}]+\}""\)\s*=\s*""[^""]*"",\s*""([^""]+)""", RegexOptions.Compiled)]
    private static partial Regex SlnProjectRegex();

    /// <summary>
    /// Source-generated regex matching .slnx XML Project elements.
    /// Captures the path attribute from: <c>&lt;Project Path="..." /&gt;</c>.
    /// </summary>
    [GeneratedRegex(@"<Project\s+Path=""([^""]+)""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex SlnxProjectRegex();
}
