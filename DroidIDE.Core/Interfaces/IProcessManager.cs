namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Abstracts OS-level process execution (running shell commands, dotnet CLI, etc.).
/// </summary>
public interface IProcessManager
{
    /// <summary>Start a process and return its output when it completes.</summary>
    Task<ProcessResult> RunAsync(string command, string arguments, string workingDirectory, CancellationToken cancellationToken = default);

    /// <summary>Start a long-running process with real-time output streaming.</summary>
    Task<IRunningProcess> StartAsync(string command, string arguments, string workingDirectory, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a completed process result.
/// </summary>
public class ProcessResult
{
    public int ExitCode { get; set; }
    public string StandardOutput { get; set; } = string.Empty;
    public string StandardError { get; set; } = string.Empty;
    public bool Success => ExitCode == 0;
}

/// <summary>
/// Represents a running process that can be interacted with.
/// </summary>
public interface IRunningProcess : IAsyncDisposable
{
    int ProcessId { get; }
    bool IsRunning { get; }
    event Action<string>? OutputReceived;
    event Action<string>? ErrorReceived;
    event Action<int>? Exited;
    Task SendInputAsync(string input);
    Task KillAsync();
    Task<int> WaitForExitAsync(CancellationToken cancellationToken = default);
}
