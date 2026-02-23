using System.Diagnostics;
using DroidIDE.Core.Enums;
using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;
using DroidIDE.Terminal.OutputParser;

namespace DroidIDE.Runtime.DotnetManager;

/// <summary>
/// Wraps all dotnet CLI operations, providing typed access to build, run, test, etc.
/// </summary>
public class DotnetCliService : IDotnetCli
{
    private readonly IProcessManager _processManager;

    public DotnetCliService(IProcessManager processManager)
    {
        _processManager = processManager;
    }

    /// <inheritdoc/>
    public event Action<string>? BuildOutputReceived;

    /// <inheritdoc/>
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
