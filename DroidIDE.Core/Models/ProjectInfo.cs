namespace DroidIDE.Core.Models;

/// <summary>
/// Represents a .NET project parsed from a .csproj file.
/// Contains project metadata, target framework, package references, and project references.
/// </summary>
public class ProjectInfo
{
    /// <summary>Gets or sets the project name (derived from the .csproj filename).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the absolute path to the .csproj file.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the target framework moniker (e.g., "net10.0-android").</summary>
    public string TargetFramework { get; set; } = string.Empty;

    /// <summary>Gets or sets the output type: "Library", "Exe", or "WinExe".</summary>
    public string OutputType { get; set; } = "Library";

    /// <summary>Gets or sets the root namespace for the project.</summary>
    public string RootNamespace { get; set; } = string.Empty;

    /// <summary>Gets or sets the MSBuild SDK identifier (e.g., "Microsoft.NET.Sdk", "Microsoft.NET.Sdk.Web").</summary>
    public string Sdk { get; set; } = string.Empty;

    /// <summary>Gets or sets the NuGet package references declared in the project file.</summary>
    public List<PackageReference> PackageReferences { get; set; } = [];

    /// <summary>Gets or sets the project-to-project references declared in the project file.</summary>
    public List<string> ProjectReferences { get; set; } = [];

    /// <summary>
    /// Gets a value indicating whether this project builds an executable (OutputType is "Exe").
    /// </summary>
    public bool IsExecutable => OutputType.Equals("Exe", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this project is an ASP.NET Core web application,
    /// determined by the SDK containing "Web".
    /// </summary>
    public bool IsWebApp => Sdk.Contains("Web", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Represents a NuGet package reference declared in a .csproj project file.
/// </summary>
public class PackageReference
{
    /// <summary>Gets or sets the NuGet package identifier (e.g., "Newtonsoft.Json").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the package version string (e.g., "13.0.3").</summary>
    public string Version { get; set; } = string.Empty;
}
