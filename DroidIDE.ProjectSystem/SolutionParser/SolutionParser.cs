using System.Text.RegularExpressions;
using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.ProjectSystem.SolutionParser;

/// <summary>
/// Parses .sln and .slnx solution files to extract project references.
/// </summary>
public partial class SolutionParser : ISolutionParser
{
    private readonly IProjectParser _projectParser;

    public SolutionParser(IProjectParser projectParser)
    {
        _projectParser = projectParser;
    }

    /// <summary>
    /// Parse a solution file and return solution metadata with project list.
    /// Supports both classic .sln format and XML-based .slnx format.
    /// </summary>
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
    /// Detect if a file is a solution file (.sln or .slnx).
    /// </summary>
    public bool IsSolutionFile(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".sln" or ".slnx";
    }

    /// <summary>
    /// Parse classic .sln format using regex to extract Project() lines.
    /// </summary>
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
    /// Parse .slnx XML format.
    /// </summary>
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

    // Classic .sln format: Project("{...}") = "Name", "Path\To\Project.csproj", "{...}"
    [GeneratedRegex(@"Project\(""\{[^}]+\}""\)\s*=\s*""[^""]*"",\s*""([^""]+)""", RegexOptions.Compiled)]
    private static partial Regex SlnProjectRegex();

    // .slnx XML format: <Project Path="..." />
    [GeneratedRegex(@"<Project\s+Path=""([^""]+)""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex SlnxProjectRegex();
}
