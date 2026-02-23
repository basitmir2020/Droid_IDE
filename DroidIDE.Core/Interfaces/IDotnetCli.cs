using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Abstracts dotnet CLI operations for building, running, testing, and managing .NET projects.
/// Implemented by <c>DotnetCliService</c> in the Runtime layer.
/// </summary>
public interface IDotnetCli
{
    /// <summary>Executes <c>dotnet build</c> on a project or solution and returns structured build results.</summary>
    /// <param name="projectPath">The path to the .csproj or .sln file to build.</param>
    /// <param name="cancellationToken">Optional cancellation token to abort the build.</param>
    /// <returns>A <see cref="BuildResult"/> containing success status, diagnostics, and duration.</returns>
    Task<BuildResult> BuildAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>Executes <c>dotnet run</c> on a project and returns a handle to the running process.</summary>
    /// <param name="projectPath">The path to the .csproj file to run.</param>
    /// <param name="cancellationToken">Optional cancellation token to abort the process.</param>
    /// <returns>An <see cref="IRunningProcess"/> for interacting with the running application.</returns>
    Task<IRunningProcess> RunAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>Executes <c>dotnet new</c> to scaffold a new project from a template.</summary>
    /// <param name="template">The template short name (e.g., "console", "webapi", "classlib").</param>
    /// <param name="outputDirectory">The directory where the project will be created.</param>
    /// <param name="name">The name for the new project.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="ProcessResult"/> with the CLI output.</returns>
    Task<ProcessResult> NewAsync(string template, string outputDirectory, string name, CancellationToken cancellationToken = default);

    /// <summary>Executes <c>dotnet restore</c> to restore NuGet packages for a project or solution.</summary>
    /// <param name="projectPath">The path to the .csproj or .sln file.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="ProcessResult"/> with the restore output.</returns>
    Task<ProcessResult> RestoreAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>Executes <c>dotnet test</c> on a test project.</summary>
    /// <param name="projectPath">The path to the test .csproj file.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="ProcessResult"/> with the test output and exit code.</returns>
    Task<ProcessResult> TestAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>Executes <c>dotnet clean</c> to remove build artifacts from a project or solution.</summary>
    /// <param name="projectPath">The path to the .csproj or .sln file.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="ProcessResult"/> with the clean output.</returns>
    Task<ProcessResult> CleanAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the installed .NET SDK version by running <c>dotnet --version</c>.</summary>
    /// <returns>The SDK version string (e.g., "10.0.100").</returns>
    Task<string> GetSdkVersionAsync();

    /// <summary>Checks whether the <c>dotnet</c> CLI executable is available on the current device.</summary>
    /// <returns><c>true</c> if the CLI is found and responsive; otherwise, <c>false</c>.</returns>
    Task<bool> IsAvailableAsync();

    /// <summary>Raised when build output lines are received in real time during a <see cref="BuildAsync"/> operation.</summary>
    event Action<string>? BuildOutputReceived;
}
