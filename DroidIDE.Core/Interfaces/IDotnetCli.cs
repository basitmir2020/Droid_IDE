using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Abstracts dotnet CLI operations (build, run, new, restore, test).
/// </summary>
public interface IDotnetCli
{
    /// <summary>Run 'dotnet build' on a project or solution.</summary>
    Task<BuildResult> BuildAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>Run 'dotnet run' on a project. Returns the running process.</summary>
    Task<IRunningProcess> RunAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>Run 'dotnet new' to create a new project from a template.</summary>
    Task<ProcessResult> NewAsync(string template, string outputDirectory, string name, CancellationToken cancellationToken = default);

    /// <summary>Run 'dotnet restore' on a project or solution.</summary>
    Task<ProcessResult> RestoreAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>Run 'dotnet test' on a test project.</summary>
    Task<ProcessResult> TestAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>Run 'dotnet clean' on a project or solution.</summary>
    Task<ProcessResult> CleanAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>Get the installed SDK version.</summary>
    Task<string> GetSdkVersionAsync();

    /// <summary>Check whether the dotnet CLI is available on this device.</summary>
    Task<bool> IsAvailableAsync();

    /// <summary>Event fired when build output lines are received in real time.</summary>
    event Action<string>? BuildOutputReceived;
}
