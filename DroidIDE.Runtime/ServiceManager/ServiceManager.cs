using System.Collections.Concurrent;
using DroidIDE.Core.Interfaces;

namespace DroidIDE.Runtime.ServiceManager;

/// <summary>
/// Tracks and manages multiple independently running .NET services (microservices).
/// </summary>
public class ServiceManager
{
    private readonly IDotnetCli _dotnetCli;
    private readonly ConcurrentDictionary<string, ServiceInstance> _services = new();

    public ServiceManager(IDotnetCli dotnetCli)
    {
        _dotnetCli = dotnetCli;
    }

    /// <summary>Currently tracked services.</summary>
    public IReadOnlyDictionary<string, ServiceInstance> Services => _services;

    /// <summary>Fired when a service starts, stops, or produces output.</summary>
    public event Action<string, string>? ServiceOutputReceived;

    /// <summary>Fired when a service exits.</summary>
    public event Action<string, int>? ServiceExited;

    /// <summary>
    /// Start a service from a project path. Uses project name as service ID.
    /// </summary>
    public async Task StartServiceAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var serviceId = Path.GetFileNameWithoutExtension(projectPath);

        // Stop existing instance if any
        await StopServiceAsync(serviceId);

        var process = await _dotnetCli.RunAsync(projectPath, cancellationToken);

        var instance = new ServiceInstance
        {
            ServiceId = serviceId,
            ProjectPath = projectPath,
            Process = process,
            StartedAt = DateTimeOffset.UtcNow
        };

        process.OutputReceived += line => ServiceOutputReceived?.Invoke(serviceId, line);
        process.ErrorReceived += line => ServiceOutputReceived?.Invoke(serviceId, $"[ERROR] {line}");
        process.Exited += code =>
        {
            instance.IsRunning = false;
            ServiceExited?.Invoke(serviceId, code);
        };

        instance.IsRunning = true;
        _services[serviceId] = instance;
    }

    /// <summary>
    /// Stop a specific service by ID.
    /// </summary>
    public async Task StopServiceAsync(string serviceId)
    {
        if (_services.TryRemove(serviceId, out var instance) && instance.Process.IsRunning)
        {
            await instance.Process.KillAsync();
            await instance.Process.DisposeAsync();
        }
    }

    /// <summary>
    /// Stop all running services.
    /// </summary>
    public async Task StopAllAsync()
    {
        foreach (var serviceId in _services.Keys.ToList())
        {
            await StopServiceAsync(serviceId);
        }
    }

    /// <summary>
    /// Get the status of all services.
    /// </summary>
    public IEnumerable<ServiceStatus> GetAllStatuses()
    {
        return _services.Values.Select(s => new ServiceStatus
        {
            ServiceId = s.ServiceId,
            ProjectPath = s.ProjectPath,
            IsRunning = s.IsRunning,
            StartedAt = s.StartedAt,
            ProcessId = s.Process.ProcessId
        });
    }
}

/// <summary>Internal tracking of a running service instance.</summary>
public class ServiceInstance
{
    public required string ServiceId { get; set; }
    public required string ProjectPath { get; set; }
    public required IRunningProcess Process { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public bool IsRunning { get; set; }
}

/// <summary>Public-facing service status information.</summary>
public class ServiceStatus
{
    public string ServiceId { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public int ProcessId { get; set; }
}
