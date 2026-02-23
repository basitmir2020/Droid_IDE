using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.ProjectSystem.NuGetManager;

/// <summary>
/// Manages NuGet package operations via <c>dotnet</c> CLI commands.
/// Provides methods to restore, add, remove, and list outdated packages.
/// Uses <see cref="IProcessManager"/> to execute the underlying CLI processes.
/// </summary>
public class NuGetManager
{
    private readonly IProcessManager _processManager;

    /// <summary>
    /// Initializes a new instance of <see cref="NuGetManager"/>.
    /// </summary>
    /// <param name="processManager">The process manager used to execute dotnet CLI commands.</param>
    public NuGetManager(IProcessManager processManager)
    {
        _processManager = processManager;
    }

    /// <summary>
    /// Runs <c>dotnet restore</c> on the specified project or solution to restore NuGet packages.
    /// </summary>
    /// <param name="projectPath">The absolute path to the .csproj or .sln file.</param>
    /// <param name="cancellationToken">Optional token to cancel the restore operation.</param>
    /// <returns>A <see cref="ProcessResult"/> with stdout, stderr, and exit code.</returns>
    public async Task<ProcessResult> RestoreAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        return await _processManager.RunAsync("dotnet", $"restore \"{projectPath}\"",
            Path.GetDirectoryName(projectPath) ?? ".", cancellationToken);
    }

    /// <summary>
    /// Adds a NuGet package to a project using <c>dotnet add package</c>.
    /// </summary>
    /// <param name="projectPath">The absolute path to the .csproj file.</param>
    /// <param name="packageId">The NuGet package identifier (e.g., "Newtonsoft.Json").</param>
    /// <param name="version">Optional specific version to install. If <c>null</c>, installs the latest stable version.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>A <see cref="ProcessResult"/> with stdout, stderr, and exit code.</returns>
    public async Task<ProcessResult> AddPackageAsync(string projectPath, string packageId, string? version = null,
        CancellationToken cancellationToken = default)
    {
        var args = $"add \"{projectPath}\" package {packageId}";
        if (!string.IsNullOrEmpty(version))
            args += $" --version {version}";

        return await _processManager.RunAsync("dotnet", args,
            Path.GetDirectoryName(projectPath) ?? ".", cancellationToken);
    }

    /// <summary>
    /// Removes a NuGet package from a project using <c>dotnet remove package</c>.
    /// </summary>
    /// <param name="projectPath">The absolute path to the .csproj file.</param>
    /// <param name="packageId">The NuGet package identifier to remove.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>A <see cref="ProcessResult"/> with stdout, stderr, and exit code.</returns>
    public async Task<ProcessResult> RemovePackageAsync(string projectPath, string packageId,
        CancellationToken cancellationToken = default)
    {
        return await _processManager.RunAsync("dotnet", $"remove \"{projectPath}\" package {packageId}",
            Path.GetDirectoryName(projectPath) ?? ".", cancellationToken);
    }

    /// <summary>
    /// Lists outdated NuGet packages for a project using <c>dotnet list package --outdated</c>.
    /// </summary>
    /// <param name="projectPath">The absolute path to the .csproj or .sln file.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>A <see cref="ProcessResult"/> whose stdout contains the outdated package report.</returns>
    public async Task<ProcessResult> ListOutdatedAsync(string projectPath,
        CancellationToken cancellationToken = default)
    {
        return await _processManager.RunAsync("dotnet", $"list \"{projectPath}\" package --outdated",
            Path.GetDirectoryName(projectPath) ?? ".", cancellationToken);
    }
}
