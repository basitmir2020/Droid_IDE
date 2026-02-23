using System.Diagnostics;
using DroidIDE.Core.Interfaces;

namespace DroidIDE.Infrastructure.Process;

/// <summary>
/// Implements IProcessManager using System.Diagnostics.Process.
/// </summary>
public class ProcessManager : IProcessManager
{
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
/// Wraps a System.Diagnostics.Process as an IRunningProcess for real-time interaction.
/// </summary>
internal class RunningProcess : IRunningProcess
{
    private readonly System.Diagnostics.Process _process;

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

    public int ProcessId => _process.Id;
    public bool IsRunning => !_process.HasExited;

    public event Action<string>? OutputReceived;
    public event Action<string>? ErrorReceived;
    public event Action<int>? Exited;

    public async Task SendInputAsync(string input)
    {
        await _process.StandardInput.WriteLineAsync(input);
        await _process.StandardInput.FlushAsync();
    }

    public Task KillAsync()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
        }
        return Task.CompletedTask;
    }

    public async Task<int> WaitForExitAsync(CancellationToken cancellationToken = default)
    {
        await _process.WaitForExitAsync(cancellationToken);
        return _process.ExitCode;
    }

    public async ValueTask DisposeAsync()
    {
        await KillAsync();
        _process.Dispose();
    }
}
