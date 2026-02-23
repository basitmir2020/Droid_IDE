using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;
using System.Collections.Concurrent;

namespace DroidIDE.Terminal.ProcessSession;

/// <summary>
/// Manages multiple terminal process sessions, each wrapping a running shell process.
/// Implements <see cref="ITerminalService"/> to provide session creation, I/O, and lifecycle management.
/// Sessions are tracked in a <see cref="ConcurrentDictionary{TKey,TValue}"/> for thread safety.
/// </summary>
public class ProcessSessionManager : ITerminalService
{
    private readonly IProcessManager _processManager;
    private readonly ConcurrentDictionary<string, TerminalProcessSession> _sessions = new();

    /// <inheritdoc />
    public event Action<string, string>? OutputReceived;

    /// <summary>
    /// Initializes a new instance of <see cref="ProcessSessionManager"/>.
    /// </summary>
    /// <param name="processManager">The process manager used to spawn shell processes.</param>
    public ProcessSessionManager(IProcessManager processManager)
    {
        _processManager = processManager;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Starts an Android shell process (<c>/system/bin/sh</c>) and wires up output/error event handlers
    /// to capture session output and forward it via the <see cref="OutputReceived"/> event.
    /// </remarks>
    public async Task<TerminalSession> CreateSessionAsync(string workingDirectory)
    {
        var session = new TerminalSession
        {
            Name = $"Terminal {_sessions.Count + 1}",
            IsRunning = true,
            WorkingDirectory = workingDirectory
        };

        // Start a shell process (sh on Android/Linux)
        var process = await _processManager.StartAsync("/system/bin/sh", "", workingDirectory);

        var processSession = new TerminalProcessSession(session, process);

        process.OutputReceived += line =>
        {
            session.OutputLines.Add(line);
            OutputReceived?.Invoke(session.Id, line);
        };

        process.ErrorReceived += line =>
        {
            session.OutputLines.Add(line);
            OutputReceived?.Invoke(session.Id, line);
        };

        process.Exited += _ =>
        {
            session.IsRunning = false;
        };

        _sessions[session.Id] = processSession;
        return session;
    }

    /// <inheritdoc />
    public async Task SendCommandAsync(string sessionId, string command)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            await session.Process.SendInputAsync(command);
        }
    }

    /// <inheritdoc />
    /// <remarks>Delegates to <see cref="SendCommandAsync"/> since raw input uses the same stdin mechanism.</remarks>
    public async Task SendInputAsync(string sessionId, string input)
    {
        await SendCommandAsync(sessionId, input);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> GetOutputAsync(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return Task.FromResult<IReadOnlyList<string>>(session.Session.OutputLines.AsReadOnly());
        }
        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }

    /// <inheritdoc />
    /// <remarks>Removes the session from tracking, kills the underlying process, and disposes OS resources.</remarks>
    public async Task CloseSessionAsync(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            session.Session.IsRunning = false;
            await session.Process.KillAsync();
            await session.Process.DisposeAsync();
        }
    }
}

/// <summary>
/// Internal record pairing a <see cref="TerminalSession"/> model with its underlying <see cref="IRunningProcess"/>.
/// Used as the value type in the session tracking dictionary.
/// </summary>
/// <param name="Session">The terminal session metadata.</param>
/// <param name="Process">The running shell process handle.</param>
internal record TerminalProcessSession(TerminalSession Session, IRunningProcess Process);
