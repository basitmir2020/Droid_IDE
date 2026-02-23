namespace DroidIDE.Core.Models;

/// <summary>
/// Represents a terminal session with a running shell process.
/// </summary>
public class TerminalSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Terminal";
    public bool IsRunning { get; set; }
    public string WorkingDirectory { get; set; } = string.Empty;
    public List<string> OutputLines { get; set; } = [];
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}
