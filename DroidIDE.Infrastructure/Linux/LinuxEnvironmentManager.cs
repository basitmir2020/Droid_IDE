namespace DroidIDE.Infrastructure.Linux;

/// <summary>
/// Manages the Linux environment configuration on Android (Termux/proot).
/// Sets up PATH, environment variables, and SDK paths.
/// </summary>
public class LinuxEnvironmentManager
{
    private readonly Dictionary<string, string> _environmentVariables = new();

    /// <summary>
    /// Configures the Linux environment for dotnet CLI execution.
    /// </summary>
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
    /// Sets an environment variable for the current process.
    /// </summary>
    public void SetVariable(string name, string value)
    {
        _environmentVariables[name] = value;
        Environment.SetEnvironmentVariable(name, value);
    }

    /// <summary>
    /// Gets the value of an environment variable.
    /// </summary>
    public string? GetVariable(string name)
    {
        return _environmentVariables.TryGetValue(name, out var value)
            ? value
            : Environment.GetEnvironmentVariable(name);
    }

    /// <summary>
    /// Returns all configured environment variables as a dictionary
    /// suitable for passing to Process.StartInfo.Environment.
    /// </summary>
    public IReadOnlyDictionary<string, string> GetAll() => _environmentVariables;

    /// <summary>
    /// Checks if the dotnet CLI is accessible from the configured PATH.
    /// </summary>
    public bool IsDotnetAvailable()
    {
        var dotnetRoot = GetVariable("DOTNET_ROOT");
        if (string.IsNullOrEmpty(dotnetRoot))
            return false;

        var dotnetPath = Path.Combine(dotnetRoot, "dotnet");
        return File.Exists(dotnetPath);
    }
}
