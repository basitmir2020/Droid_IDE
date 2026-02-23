using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Parses .csproj project files to extract project metadata, target frameworks, and package references.
/// Implemented by <c>ProjectParser</c> in the ProjectSystem layer.
/// </summary>
public interface IProjectParser
{
    /// <summary>Parses a .csproj file and extracts all project metadata.</summary>
    /// <param name="csprojPath">The absolute path to the .csproj file.</param>
    /// <returns>A <see cref="ProjectInfo"/> containing the parsed project metadata.</returns>
    Task<ProjectInfo> ParseAsync(string csprojPath);

    /// <summary>Extracts all NuGet package references from a .csproj file.</summary>
    /// <param name="csprojPath">The absolute path to the .csproj file.</param>
    /// <returns>A list of <see cref="PackageReference"/> entries with package names and versions.</returns>
    Task<List<PackageReference>> GetPackageReferencesAsync(string csprojPath);
}
