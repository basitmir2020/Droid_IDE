using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Manages terminal shell sessions for interactive command execution.
/// </summary>
public interface ITerminalService
{
    /// <summary>Create and start a new terminal session.</summary>
    Task<TerminalSession> CreateSessionAsync(string workingDirectory);

    /// <summary>Send a command to an existing terminal session.</summary>
    Task SendCommandAsync(string sessionId, string command);

    /// <summary>Send raw input (including keystrokes) to a session.</summary>
    Task SendInputAsync(string sessionId, string input);

    /// <summary>Get the output buffer for a session.</summary>
    Task<IReadOnlyList<string>> GetOutputAsync(string sessionId);

    /// <summary>Close and dispose a terminal session.</summary>
    Task CloseSessionAsync(string sessionId);

    /// <summary>Event fired when new output is available from any session.</summary>
    event Action<string, string>? OutputReceived; // sessionId, line
}
