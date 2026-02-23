namespace DroidIDE.Infrastructure.Linux;

/// <summary>
/// Manages the Linux environment configuration on Android (Termux/proot).
/// Sets up PATH, DOTNET_ROOT, HOME, and other environment variables required for .NET SDK execution.
/// </summary>
public class LinuxEnvironmentManager
{
    /// <summary>
    /// The in-process cache of configured environment variables.
    /// </summary>
    private readonly Dictionary<string, string> _environmentVariables = new();

    /// <summary>
    /// Configures the Linux environment for dotnet CLI execution by setting required environment variables.
    /// </summary>
    /// <param name="dotnetSdkPath">The absolute path to the .NET SDK installation directory.</param>
    /// <param name="homePath">The absolute path to use as the HOME directory.</param>
    /// <remarks>
    /// Sets DOTNET_ROOT, HOME, disables telemetry (DOTNET_CLI_TELEMETRY_OPTOUT),
    /// suppresses the dotnet logo (DOTNET_NOLOGO), and prepends the SDK directory to PATH.
    /// </remarks>
    public void Configure(string dotnetSdkPath, string homePath)
    {
        SetVariable("DOTNET_ROOT", dotnetSdkPath);
        SetVariable("HOME", homePath);
        SetVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");
        SetVariable("DOTNET_NOLOGO", "1");

        // Prepend dotnet to PATH
        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
        SetVariable("PATH", $"{dotnetSdkPath}:{currentPath}");
    }

    /// <summary>
    /// Sets an environment variable for the current process and caches it in the local dictionary.
    /// </summary>
    /// <param name="name">The environment variable name.</param>
    /// <param name="value">The environment variable value.</param>
    public void SetVariable(string name, string value)
    {
        _environmentVariables[name] = value;
        Environment.SetEnvironmentVariable(name, value);
    }

    /// <summary>
    /// Gets the value of an environment variable, checking the local cache first, then the OS environment.
    /// </summary>
    /// <param name="name">The environment variable name to retrieve.</param>
    /// <returns>The variable value, or <c>null</c> if not found in either source.</returns>
    public string? GetVariable(string name)
    {
        return _environmentVariables.TryGetValue(name, out var value)
            ? value
            : Environment.GetEnvironmentVariable(name);
    }

    /// <summary>
    /// Returns all configured environment variables as a read-only dictionary,
    /// suitable for passing to <c>ProcessStartInfo.Environment</c>.
    /// </summary>
    /// <returns>A read-only dictionary of environment variable names and values.</returns>
    public IReadOnlyDictionary<string, string> GetAll() => _environmentVariables;

    /// <summary>
    /// Checks if the <c>dotnet</c> CLI executable is accessible from the configured DOTNET_ROOT path.
    /// </summary>
    /// <returns><c>true</c> if the dotnet executable exists at the configured path; otherwise, <c>false</c>.</returns>
    public bool IsDotnetAvailable()
    {
        var dotnetRoot = GetVariable("DOTNET_ROOT");
        if (string.IsNullOrEmpty(dotnetRoot))
            return false;

        var dotnetPath = Path.Combine(dotnetRoot, "dotnet");
        return File.Exists(dotnetPath);
    }
}
