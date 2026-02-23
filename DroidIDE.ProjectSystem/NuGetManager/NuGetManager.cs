using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.ProjectSystem.NuGetManager;

/// <summary>
/// Manages NuGet packages via dotnet CLI commands.
/// </summary>
public class NuGetManager
{
    private readonly IProcessManager _processManager;

    public NuGetManager(IProcessManager processManager)
    {
        _processManager = processManager;
    }

    /// <summary>
    /// Run dotnet restore on a project or solution.
    /// </summary>
    public async Task<ProcessResult> RestoreAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        return await _processManager.RunAsync("dotnet", $"restore \"{projectPath}\"",
            Path.GetDirectoryName(projectPath) ?? ".", cancellationToken);
    }

    /// <summary>
    /// Add a NuGet package to a project.
    /// </summary>
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
    /// Remove a NuGet package from a project.
    /// </summary>
    public async Task<ProcessResult> RemovePackageAsync(string projectPath, string packageId,
        CancellationToken cancellationToken = default)
    {
        return await _processManager.RunAsync("dotnet", $"remove \"{projectPath}\" package {packageId}",
            Path.GetDirectoryName(projectPath) ?? ".", cancellationToken);
    }

    /// <summary>
    /// List outdated packages for a project.
    /// </summary>
    public async Task<ProcessResult> ListOutdatedAsync(string projectPath,
        CancellationToken cancellationToken = default)
    {
        return await _processManager.RunAsync("dotnet", $"list \"{projectPath}\" package --outdated",
            Path.GetDirectoryName(projectPath) ?? ".", cancellationToken);
    }
}
