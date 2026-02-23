using System.Diagnostics;
using DroidIDE.Core.Enums;
using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;
using DroidIDE.Terminal.OutputParser;

namespace DroidIDE.Runtime.DotnetManager;

/// <summary>
/// Implements <see cref="IDotnetCli"/> by wrapping all dotnet CLI operations.
/// Provides typed access to build, run, test, clean, restore, and SDK version commands.
/// Uses <see cref="IProcessManager"/> for process execution and <see cref="OutputLineParser"/>
/// for extracting structured diagnostics from build output.
/// </summary>
public class DotnetCliService : IDotnetCli
{
    private readonly IProcessManager _processManager;

    /// <summary>
    /// Initializes a new instance of <see cref="DotnetCliService"/>.
    /// </summary>
    /// <param name="processManager">The process manager used for executing dotnet CLI commands.</param>
    public DotnetCliService(IProcessManager processManager)
    {
        _processManager = processManager;
    }

    /// <inheritdoc/>
    public event Action<string>? BuildOutputReceived;

    /// <inheritdoc/>
    /// <remarks>
    /// Starts a streaming <c>dotnet build --no-restore</c> process, captures each output line,
    /// parses MSBuild diagnostics via <see cref="OutputLineParser.TryParseDiagnostic"/>,
    /// and waits for the process to exit before returning a structured <see cref="BuildResult"/>.
    /// </remarks>
    public async Task<BuildResult> BuildAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var diagnostics = new List<DiagnosticItem>();
        var outputLines = new List<string>();

        var workingDir = Path.GetDirectoryName(projectPath) ?? ".";
        var process = await _processManager.StartAsync("dotnet", $"build \"{projectPath}\" --no-restore",
            workingDir, cancellationToken);

        var tcs = new TaskCompletionSource<int>();

        process.OutputReceived += line =>
        {
            outputLines.Add(line);
            BuildOutputReceived?.Invoke(line);

            // Try to parse MSBuild diagnostics from each output line
            var diagnostic = OutputLineParser.TryParseDiagnostic(line);
            if (diagnostic is not null)
            {
                diagnostics.Add(diagnostic);
            }
        };

        process.ErrorReceived += line =>
        {
            outputLines.Add(line);
            BuildOutputReceived?.Invoke(line);
        };

        process.Exited += exitCode => tcs.TrySetResult(exitCode);

        var code = await tcs.Task;
        stopwatch.Stop();

        return new BuildResult
        {
            Success = code == 0,
            Status = code == 0 ? BuildStatus.Success : BuildStatus.Failed,
            Diagnostics = diagnostics,
            OutputLines = outputLines,
            Duration = stopwatch.Elapsed
        };
    }

    /// <inheritdoc/>
    public async Task<IRunningProcess> RunAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var workingDir = Path.GetDirectoryName(projectPath) ?? ".";
        return await _processManager.StartAsync("dotnet", $"run --project \"{projectPath}\"",
            workingDir, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProcessResult> NewAsync(string template, string outputDirectory, string name,
        CancellationToken cancellationToken = default)
    {
        return await _processManager.RunAsync("dotnet", $"new {template} -n \"{name}\" -o \"{outputDirectory}\"",
            outputDirectory, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProcessResult> RestoreAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var workingDir = Path.GetDirectoryName(projectPath) ?? ".";
        return await _processManager.RunAsync("dotnet", $"restore \"{projectPath}\"",
            workingDir, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProcessResult> TestAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var workingDir = Path.GetDirectoryName(projectPath) ?? ".";
        return await _processManager.RunAsync("dotnet", $"test \"{projectPath}\"",
            workingDir, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProcessResult> CleanAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var workingDir = Path.GetDirectoryName(projectPath) ?? ".";
        return await _processManager.RunAsync("dotnet", $"clean \"{projectPath}\"",
            workingDir, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string> GetSdkVersionAsync()
    {
        var result = await _processManager.RunAsync("dotnet", "--version", ".");
        return result.StandardOutput.Trim();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Runs <c>dotnet --version</c> and considers the SDK available if the command succeeds
    /// and produces non-empty output. Returns <c>false</c> on any exception (e.g., dotnet not found).
    /// </remarks>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var result = await _processManager.RunAsync("dotnet", "--version", ".");
            return result.Success && !string.IsNullOrWhiteSpace(result.StandardOutput);
        }
        catch
        {
            return false;
        }
    }
}
