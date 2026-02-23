namespace DroidIDE.Core.Models;

/// <summary>
/// Represents an active terminal session backed by a running shell process.
/// Tracks session metadata, output history, and lifecycle state.
/// </summary>
public class TerminalSession
{
    /// <summary>Gets or sets the unique session identifier, generated via <see cref="Guid.NewGuid"/>.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the display name for this session (e.g., "Terminal", "Terminal 2").</summary>
    public string Name { get; set; } = "Terminal";

    /// <summary>Gets or sets a value indicating whether the underlying shell process is currently running.</summary>
    public bool IsRunning { get; set; }

    /// <summary>Gets or sets the current working directory of the shell process.</summary>
    public string WorkingDirectory { get; set; } = string.Empty;

    /// <summary>Gets or sets the accumulated output lines from the shell process.</summary>
    public List<string> OutputLines { get; set; } = [];

    /// <summary>Gets or sets the UTC timestamp when this session was created.</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}
