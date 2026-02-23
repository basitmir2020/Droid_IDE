using System.Collections.Concurrent;
using DroidIDE.Core.Interfaces;

namespace DroidIDE.Runtime.ServiceManager;

/// <summary>
/// Tracks and manages multiple independently running .NET services (microservices).
/// Uses <see cref="IDotnetCli.RunAsync"/> to start each service and a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> for thread-safe session tracking.
/// </summary>
public class ServiceManager
{
    private readonly IDotnetCli _dotnetCli;
    private readonly ConcurrentDictionary<string, ServiceInstance> _services = new();

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceManager"/>.
    /// </summary>
    /// <param name="dotnetCli">The dotnet CLI service used to run service projects.</param>
    public ServiceManager(IDotnetCli dotnetCli)
    {
        _dotnetCli = dotnetCli;
    }

    /// <summary>Gets a read-only view of all currently tracked service instances.</summary>
    public IReadOnlyDictionary<string, ServiceInstance> Services => _services;

    /// <summary>
    /// Raised when a service produces output. Parameters are (serviceId, outputLine).
    /// Error output lines are prefixed with <c>[ERROR]</c>.
    /// </summary>
    public event Action<string, string>? ServiceOutputReceived;

    /// <summary>Raised when a service process exits. Parameters are (serviceId, exitCode).</summary>
    public event Action<string, int>? ServiceExited;

    /// <summary>
    /// Starts a .NET service from the specified project path.
    /// Uses the project file name (without extension) as the service identifier.
    /// Any previously running instance with the same ID is stopped first.
    /// </summary>
    /// <param name="projectPath">The absolute path to the .csproj file of the service to start.</param>
    /// <param name="cancellationToken">Optional token to cancel the start operation.</param>
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
    /// Stops a specific service by its identifier, killing the process and releasing resources.
    /// </summary>
    /// <param name="serviceId">The service identifier (project name without extension).</param>
    /// <remarks>No-op if the service is not found or is already stopped.</remarks>
    public async Task StopServiceAsync(string serviceId)
    {
        if (_services.TryRemove(serviceId, out var instance) && instance.Process.IsRunning)
        {
            await instance.Process.KillAsync();
            await instance.Process.DisposeAsync();
        }
    }

    /// <summary>
    /// Stops all currently running services.
    /// </summary>
    public async Task StopAllAsync()
    {
        foreach (var serviceId in _services.Keys.ToList())
        {
            await StopServiceAsync(serviceId);
        }
    }

    /// <summary>
    /// Returns status information for all tracked services as <see cref="ServiceStatus"/> DTOs.
    /// </summary>
    /// <returns>An enumerable of <see cref="ServiceStatus"/> with current state details for each service.</returns>
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

/// <summary>
/// Internal tracking of a running service instance, pairing metadata with the underlying process handle.
/// </summary>
public class ServiceInstance
{
    /// <summary>Gets or sets the unique service identifier (derived from the project name).</summary>
    public required string ServiceId { get; set; }

    /// <summary>Gets or sets the absolute path to the .csproj file of the service.</summary>
    public required string ProjectPath { get; set; }

    /// <summary>Gets or sets the running process handle for the service.</summary>
    public required IRunningProcess Process { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the service was started.</summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>Gets or sets whether the service is currently running.</summary>
    public bool IsRunning { get; set; }
}

/// <summary>
/// Public-facing DTO providing status information about a managed service instance.
/// </summary>
public class ServiceStatus
{
    /// <summary>Gets or sets the unique service identifier.</summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the absolute path to the service's .csproj file.</summary>
    public string ProjectPath { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the service is currently running.</summary>
    public bool IsRunning { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the service was started.</summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>Gets or sets the OS process ID of the running service.</summary>
    public int ProcessId { get; set; }
}
