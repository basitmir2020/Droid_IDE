using System.Diagnostics;
using DroidIDE.Core.Interfaces;

namespace DroidIDE.Infrastructure.Process;

/// <summary>
/// Implements <see cref="IProcessManager"/> using <see cref="System.Diagnostics.Process"/>
/// for executing OS-level processes in both run-to-completion and streaming modes.
/// </summary>
public class ProcessManager : IProcessManager
{
    /// <inheritdoc />
    /// <remarks>
    /// Starts the process, captures all stdout and stderr, waits for completion, and returns the combined result.
    /// Suitable for short-lived commands like <c>dotnet build</c> or <c>git status</c>.
    /// </remarks>
    public async Task<ProcessResult> RunAsync(string command, string arguments, string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        using var process = new System.Diagnostics.Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = stdout,
            StandardError = stderr
        };
    }

    /// <inheritdoc />
    /// <remarks>
    /// Starts an interactive process with stdin/stdout/stderr redirection and begins asynchronous output reading.
    /// Suitable for long-running commands like <c>dotnet run</c> or interactive shell sessions.
    /// </remarks>
    public Task<IRunningProcess> StartAsync(string command, string arguments, string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        var process = new System.Diagnostics.Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var runningProcess = new RunningProcess(process);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return Task.FromResult<IRunningProcess>(runningProcess);
    }
}

/// <summary>
/// Wraps a <see cref="System.Diagnostics.Process"/> instance as an <see cref="IRunningProcess"/>
/// for real-time output streaming, stdin interaction, and lifecycle control.
/// </summary>
internal class RunningProcess : IRunningProcess
{
    private readonly System.Diagnostics.Process _process;

    /// <summary>
    /// Initializes a new instance of <see cref="RunningProcess"/> and wires up output/error/exit event handlers.
    /// </summary>
    /// <param name="process">The underlying OS process to wrap.</param>
    public RunningProcess(System.Diagnostics.Process process)
    {
        _process = process;

        _process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                OutputReceived?.Invoke(e.Data);
        };

        _process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                ErrorReceived?.Invoke(e.Data);
        };

        _process.Exited += (_, _) =>
        {
            Exited?.Invoke(_process.ExitCode);
        };

        _process.EnableRaisingEvents = true;
    }

    /// <inheritdoc />
    public int ProcessId => _process.Id;

    /// <inheritdoc />
    public bool IsRunning => !_process.HasExited;

    /// <inheritdoc />
    public event Action<string>? OutputReceived;

    /// <inheritdoc />
    public event Action<string>? ErrorReceived;

    /// <inheritdoc />
    public event Action<int>? Exited;

    /// <inheritdoc />
    /// <remarks>Writes the input followed by a newline and flushes the stdin stream.</remarks>
    public async Task SendInputAsync(string input)
    {
        await _process.StandardInput.WriteLineAsync(input);
        await _process.StandardInput.FlushAsync();
    }

    /// <inheritdoc />
    /// <remarks>Kills the entire process tree to ensure child processes are also terminated.</remarks>
    public Task KillAsync()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<int> WaitForExitAsync(CancellationToken cancellationToken = default)
    {
        await _process.WaitForExitAsync(cancellationToken);
        return _process.ExitCode;
    }

    /// <summary>
    /// Disposes the running process by killing it (if still running) and releasing OS resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await KillAsync();
        _process.Dispose();
    }
}
