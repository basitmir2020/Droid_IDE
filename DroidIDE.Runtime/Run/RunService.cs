using DroidIDE.Core.Interfaces;

namespace DroidIDE.Runtime.Run;

/// <summary>
/// Manages the lifecycle of a running .NET application including starting, stopping, and restarting.
/// Wraps <see cref="IDotnetCli.RunAsync"/> and provides event-based output forwarding.
/// Only one application may run at a time; starting a new one stops the previous instance.
/// </summary>
public class RunService
{
    private readonly IDotnetCli _dotnetCli;
    private IRunningProcess? _currentProcess;

    /// <summary>
    /// Initializes a new instance of <see cref="RunService"/>.
    /// </summary>
    /// <param name="dotnetCli">The dotnet CLI service used to run projects.</param>
    public RunService(IDotnetCli dotnetCli)
    {
        _dotnetCli = dotnetCli;
    }

    /// <summary>Gets whether a .NET application is currently running.</summary>
    public bool IsRunning => _currentProcess?.IsRunning ?? false;

    /// <summary>Gets the OS process ID of the running application, or <c>null</c> if nothing is running.</summary>
    public int? ProcessId => _currentProcess?.ProcessId;

    /// <summary>Raised when the running application produces standard output.</summary>
    public event Action<string>? OutputReceived;

    /// <summary>Raised when the running application produces standard error output.</summary>
    public event Action<string>? ErrorReceived;

    /// <summary>Raised when the running application exits, providing the exit code.</summary>
    public event Action<int>? Exited;

    /// <summary>
    /// Starts a .NET project. If another instance is already running, it is stopped first.
    /// </summary>
    /// <param name="projectPath">The absolute path to the .csproj file to run.</param>
    /// <param name="cancellationToken">Optional token to cancel the start operation.</param>
    /// <remarks>
    /// Wires up <see cref="OutputReceived"/>, <see cref="ErrorReceived"/>, and <see cref="Exited"/>
    /// events from the underlying process. The <c>_currentProcess</c> reference is cleared on exit.
    /// </remarks>
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
    /// Stops the currently running application by killing its process and releasing resources.
    /// </summary>
    /// <remarks>No-op if no application is currently running.</remarks>
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
    /// Restarts the application by stopping the current instance and starting a new one.
    /// </summary>
    /// <param name="projectPath">The absolute path to the .csproj file to restart.</param>
    /// <param name="cancellationToken">Optional token to cancel the restart operation.</param>
    public async Task RestartAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        await StopAsync();
        await StartAsync(projectPath, cancellationToken);
    }

    /// <summary>
    /// Sends text input to the running application's standard input stream.
    /// </summary>
    /// <param name="input">The text to write to stdin.</param>
    /// <remarks>No-op if no application is currently running.</remarks>
    public async Task SendInputAsync(string input)
    {
        if (_currentProcess is { IsRunning: true })
        {
            await _currentProcess.SendInputAsync(input);
        }
    }
}
