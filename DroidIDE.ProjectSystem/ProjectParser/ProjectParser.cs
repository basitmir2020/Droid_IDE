using System.Xml.Linq;
using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.ProjectSystem.ProjectParser;

/// <summary>
/// Parses .csproj XML files to extract project metadata, references, and configuration.
/// </summary>
public class ProjectParser : IProjectParser
{
    /// <summary>
    /// Parse a .csproj file and return project metadata.
    /// </summary>
    public async Task<ProjectInfo> ParseAsync(string csprojPath)
    {
        if (!File.Exists(csprojPath))
            throw new FileNotFoundException("Project file not found.", csprojPath);

        var xml = await File.ReadAllTextAsync(csprojPath);
        var doc = XDocument.Parse(xml);
        var root = doc.Root;

        if (root is null)
            throw new InvalidOperationException($"Invalid .csproj file: {csprojPath}");

        // Get Sdk attribute from <Project Sdk="...">
        var sdk = root.Attribute("Sdk")?.Value ?? string.Empty;

        // Find the first PropertyGroup for project-level settings
        var ns = root.GetDefaultNamespace();
        var propertyGroups = root.Descendants(ns + "PropertyGroup");

        var targetFramework = string.Empty;
        var outputType = "Library";
        var rootNamespace = string.Empty;

        foreach (var pg in propertyGroups)
        {
            targetFramework = pg.Element(ns + "TargetFramework")?.Value
                ?? pg.Element(ns + "TargetFrameworks")?.Value?.Split(';').FirstOrDefault()
                ?? targetFramework;

            outputType = pg.Element(ns + "OutputType")?.Value ?? outputType;
            rootNamespace = pg.Element(ns + "RootNamespace")?.Value ?? rootNamespace;
        }

        // Parse PackageReferences
        var packageRefs = root.Descendants(ns + "PackageReference")
            .Select(pr => new PackageReference
            {
                Name = pr.Attribute("Include")?.Value ?? string.Empty,
                Version = pr.Attribute("Version")?.Value
                    ?? pr.Element(ns + "Version")?.Value
                    ?? string.Empty
            })
            .Where(pr => !string.IsNullOrEmpty(pr.Name))
            .ToList();

        // Parse ProjectReferences
        var projectRefs = root.Descendants(ns + "ProjectReference")
            .Select(pr => pr.Attribute("Include")?.Value ?? string.Empty)
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => p.Replace('\\', Path.DirectorySeparatorChar))
            .ToList();

        return new ProjectInfo
        {
            Name = Path.GetFileNameWithoutExtension(csprojPath),
            FilePath = csprojPath,
            TargetFramework = targetFramework,
            OutputType = outputType,
            RootNamespace = string.IsNullOrEmpty(rootNamespace)
                ? Path.GetFileNameWithoutExtension(csprojPath)
                : rootNamespace,
            Sdk = sdk,
            PackageReferences = packageRefs,
            ProjectReferences = projectRefs
        };
    }

    /// <summary>
    /// Get all package references from a .csproj file.
    /// </summary>
    public async Task<List<PackageReference>> GetPackageReferencesAsync(string csprojPath)
    {
        var project = await ParseAsync(csprojPath);
        return project.PackageReferences;
    }
}
