using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Manages interactive terminal shell sessions for command execution within the IDE.
/// Implemented by <c>ProcessSessionManager</c> in the Terminal layer.
/// </summary>
public interface ITerminalService
{
    /// <summary>Creates and starts a new terminal session with a shell process.</summary>
    /// <param name="workingDirectory">The initial working directory for the shell.</param>
    /// <returns>A <see cref="TerminalSession"/> representing the new session.</returns>
    Task<TerminalSession> CreateSessionAsync(string workingDirectory);

    /// <summary>Sends a command string to an existing terminal session, appending a newline.</summary>
    /// <param name="sessionId">The unique identifier of the target session.</param>
    /// <param name="command">The command text to execute.</param>
    Task SendCommandAsync(string sessionId, string command);

    /// <summary>Sends raw input (including control characters and keystrokes) to a session.</summary>
    /// <param name="sessionId">The unique identifier of the target session.</param>
    /// <param name="input">The raw input text to send to stdin.</param>
    Task SendInputAsync(string sessionId, string input);

    /// <summary>Retrieves the accumulated output buffer for a terminal session.</summary>
    /// <param name="sessionId">The unique identifier of the target session.</param>
    /// <returns>A read-only list of output lines from the session.</returns>
    Task<IReadOnlyList<string>> GetOutputAsync(string sessionId);

    /// <summary>Closes and disposes a terminal session, killing the underlying process.</summary>
    /// <param name="sessionId">The unique identifier of the session to close.</param>
    Task CloseSessionAsync(string sessionId);

    /// <summary>
    /// Raised when new output is received from any active session.
    /// The first parameter is the session ID; the second is the output line.
    /// </summary>
    event Action<string, string>? OutputReceived;
}
