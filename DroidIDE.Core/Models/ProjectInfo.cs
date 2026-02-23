namespace DroidIDE.Core.Models;

/// <summary>
/// Represents a .NET project parsed from a .csproj file.
/// </summary>
public class ProjectInfo
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = string.Empty;
    public string OutputType { get; set; } = "Library";
    public string RootNamespace { get; set; } = string.Empty;
    public string Sdk { get; set; } = string.Empty;
    public List<PackageReference> PackageReferences { get; set; } = [];
    public List<string> ProjectReferences { get; set; } = [];

    /// <summary>
    /// Whether this project builds an executable.
    /// </summary>
    public bool IsExecutable => OutputType.Equals("Exe", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether this project is an ASP.NET Core web application.
    /// </summary>
    public bool IsWebApp => Sdk.Contains("Web", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Represents a NuGet package reference in a project.
/// </summary>
public class PackageReference
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
