namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Abstracts OS-level process execution for running shell commands, dotnet CLI, and interactive processes.
/// Implemented by <c>ProcessManager</c> in the Infrastructure layer.
/// </summary>
public interface IProcessManager
{
    /// <summary>Starts a process, waits for it to complete, and returns the captured output.</summary>
    /// <param name="command">The executable or command to run.</param>
    /// <param name="arguments">The command-line arguments.</param>
    /// <param name="workingDirectory">The working directory for the process.</param>
    /// <param name="cancellationToken">Optional cancellation token to abort the process.</param>
    /// <returns>A <see cref="ProcessResult"/> containing exit code, stdout, and stderr.</returns>
    Task<ProcessResult> RunAsync(string command, string arguments, string workingDirectory, CancellationToken cancellationToken = default);

    /// <summary>Starts a long-running process with real-time output streaming via events.</summary>
    /// <param name="command">The executable or command to run.</param>
    /// <param name="arguments">The command-line arguments.</param>
    /// <param name="workingDirectory">The working directory for the process.</param>
    /// <param name="cancellationToken">Optional cancellation token to abort the process.</param>
    /// <returns>An <see cref="IRunningProcess"/> handle for interacting with the live process.</returns>
    Task<IRunningProcess> StartAsync(string command, string arguments, string workingDirectory, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a completed process execution, containing exit code and captured output streams.
/// </summary>
public class ProcessResult
{
    /// <summary>Gets or sets the process exit code. Zero typically indicates success.</summary>
    public int ExitCode { get; set; }

    /// <summary>Gets or sets the captured standard output text.</summary>
    public string StandardOutput { get; set; } = string.Empty;

    /// <summary>Gets or sets the captured standard error text.</summary>
    public string StandardError { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether the process completed successfully (exit code 0).</summary>
    public bool Success => ExitCode == 0;
}

/// <summary>
/// Represents a running process that supports real-time output streaming, stdin input, and lifecycle control.
/// Implements <see cref="IAsyncDisposable"/> to ensure proper cleanup of OS process resources.
/// </summary>
public interface IRunningProcess : IAsyncDisposable
{
    /// <summary>Gets the operating system process identifier.</summary>
    int ProcessId { get; }

    /// <summary>Gets a value indicating whether the process is still running.</summary>
    bool IsRunning { get; }

    /// <summary>Raised when a line of text is received from the process's standard output.</summary>
    event Action<string>? OutputReceived;

    /// <summary>Raised when a line of text is received from the process's standard error.</summary>
    event Action<string>? ErrorReceived;

    /// <summary>Raised when the process exits. The integer parameter is the exit code.</summary>
    event Action<int>? Exited;

    /// <summary>Sends text input to the process's standard input stream.</summary>
    /// <param name="input">The text to write to stdin.</param>
    Task SendInputAsync(string input);

    /// <summary>Forcefully terminates the running process.</summary>
    Task KillAsync();

    /// <summary>Waits asynchronously for the process to exit.</summary>
    /// <param name="cancellationToken">Optional cancellation token to stop waiting.</param>
    /// <returns>The process exit code.</returns>
    Task<int> WaitForExitAsync(CancellationToken cancellationToken = default);
}
