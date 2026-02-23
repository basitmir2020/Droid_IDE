using System.Xml.Linq;
using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.ProjectSystem.ProjectParser;

/// <summary>
/// Implements <see cref="IProjectParser"/> by parsing .csproj XML files using <see cref="XDocument"/>.
/// Extracts project metadata, target framework, output type, namespace, package references, and project references.
/// </summary>
public class ProjectParser : IProjectParser
{
    /// <summary>
    /// Parses a .csproj file and returns structured project metadata.
    /// </summary>
    /// <param name="csprojPath">The absolute path to the .csproj file.</param>
    /// <returns>A <see cref="ProjectInfo"/> containing the project's metadata, references, and configuration.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified .csproj file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the XML root element is missing or invalid.</exception>
    /// <remarks>
    /// Reads the Sdk attribute from the root <c>&lt;Project&gt;</c> element, iterates all
    /// <c>&lt;PropertyGroup&gt;</c> blocks for configuration, and collects both
    /// <c>&lt;PackageReference&gt;</c> and <c>&lt;ProjectReference&gt;</c> items.
    /// </remarks>
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
    /// Extracts all NuGet package references from a .csproj file.
    /// </summary>
    /// <param name="csprojPath">The absolute path to the .csproj file.</param>
    /// <returns>A list of <see cref="PackageReference"/> with package names and versions.</returns>
    /// <remarks>Delegates to <see cref="ParseAsync"/> and returns the <see cref="ProjectInfo.PackageReferences"/>.</remarks>
    public async Task<List<PackageReference>> GetPackageReferencesAsync(string csprojPath)
    {
        var project = await ParseAsync(csprojPath);
        return project.PackageReferences;
    }
}
