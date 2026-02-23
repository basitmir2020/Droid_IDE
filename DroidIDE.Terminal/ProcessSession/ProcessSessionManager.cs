using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;
using System.Collections.Concurrent;

namespace DroidIDE.Terminal.ProcessSession;

/// <summary>
/// Manages multiple terminal process sessions, each wrapping a running shell.
/// </summary>
public class ProcessSessionManager : ITerminalService
{
    private readonly IProcessManager _processManager;
    private readonly ConcurrentDictionary<string, TerminalProcessSession> _sessions = new();

    public event Action<string, string>? OutputReceived;

    public ProcessSessionManager(IProcessManager processManager)
    {
        _processManager = processManager;
    }

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

    public async Task SendCommandAsync(string sessionId, string command)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            await session.Process.SendInputAsync(command);
        }
    }

    public async Task SendInputAsync(string sessionId, string input)
    {
        await SendCommandAsync(sessionId, input);
    }

    public Task<IReadOnlyList<string>> GetOutputAsync(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return Task.FromResult<IReadOnlyList<string>>(session.Session.OutputLines.AsReadOnly());
        }
        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }

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
/// Pairs a TerminalSession model with its underlying running process.
/// </summary>
internal record TerminalProcessSession(TerminalSession Session, IRunningProcess Process);
