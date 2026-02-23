namespace DroidIDE.Core.Constants;

/// <summary>
/// Application-wide constants for configuration defaults and resource limits.
/// </summary>
public static class AppConstants
{
    /// <summary>The application display name.</summary>
    public const string AppName = "DroidIDE";

    /// <summary>The current semantic version of the application.</summary>
    public const string AppVersion = "1.0.0";

    /// <summary>Default theme to apply on first launch ("Dark" or "Light").</summary>
    public const string DefaultTheme = "Dark";

    /// <summary>Maximum number of editor tabs allowed open simultaneously to limit memory usage.</summary>
    public const int MaxOpenTabs = 20;

    /// <summary>Maximum number of output lines retained in a terminal session buffer before truncation.</summary>
    public const int TerminalBufferSize = 10_000;

    /// <summary>Debounce interval in milliseconds for file system watcher events to avoid event flooding.</summary>
    public const int FileWatcherDebounceMs = 300;

    /// <summary>Auto-save interval in milliseconds. Set to 0 to disable auto-save.</summary>
    public const int AutoSaveIntervalMs = 5_000;
}
