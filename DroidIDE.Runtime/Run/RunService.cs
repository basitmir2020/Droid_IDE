using DroidIDE.Core.Interfaces;

namespace DroidIDE.Runtime.Run;

/// <summary>
/// Manages running .NET applications with process lifecycle control.
/// Supports starting, stopping, and restarting applications.
/// </summary>
public class RunService
{
    private readonly IDotnetCli _dotnetCli;
    private IRunningProcess? _currentProcess;

    public RunService(IDotnetCli dotnetCli)
    {
        _dotnetCli = dotnetCli;
    }

    /// <summary>Whether an application is currently running.</summary>
    public bool IsRunning => _currentProcess?.IsRunning ?? false;

    /// <summary>Process ID of the running application.</summary>
    public int? ProcessId => _currentProcess?.ProcessId;

    /// <summary>Fired when the application produces output.</summary>
    public event Action<string>? OutputReceived;

    /// <summary>Fired when the application produces error output.</summary>
    public event Action<string>? ErrorReceived;

    /// <summary>Fired when the application exits.</summary>
    public event Action<int>? Exited;

    /// <summary>
    /// Start running a .NET project. Stops any previously running instance.
    /// </summary>
    public async Task StartAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        // Stop any existing process first
        await StopAsync();

        _currentProcess = await _dotnetCli.RunAsync(projectPath, cancellationToken);

        _currentProcess.OutputReceived += line => OutputReceived?.Invoke(line);
        _currentProcess.ErrorReceived += line => ErrorReceived?.Invoke(line);
        _currentProcess.Exited += code =>
        {
            Exited?.Invoke(code);
            _currentProcess = null;
        };
    }

    /// <summary>
    /// Stop the currently running application.
    /// </summary>
    public async Task StopAsync()
    {
        if (_currentProcess is { IsRunning: true })
        {
            await _currentProcess.KillAsync();
            await _currentProcess.DisposeAsync();
            _currentProcess = null;
        }
    }

    /// <summary>
    /// Restart the application (stop then start).
    /// </summary>
    public async Task RestartAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        await StopAsync();
        await StartAsync(projectPath, cancellationToken);
    }

    /// <summary>
    /// Send input to the running application's stdin.
    /// </summary>
    public async Task SendInputAsync(string input)
    {
        if (_currentProcess is { IsRunning: true })
        {
            await _currentProcess.SendInputAsync(input);
        }
    }
}
