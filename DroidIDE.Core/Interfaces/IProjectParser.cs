using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Parses .csproj project files to extract project metadata.
/// </summary>
public interface IProjectParser
{
    /// <summary>Parse a .csproj file and return project metadata.</summary>
    Task<ProjectInfo> ParseAsync(string csprojPath);

    /// <summary>Get all package references from a .csproj file.</summary>
    Task<List<PackageReference>> GetPackageReferencesAsync(string csprojPath);
}
