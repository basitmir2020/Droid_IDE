namespace DroidIDE.Core.Constants;

/// <summary>
/// Application-wide constants.
/// </summary>
public static class AppConstants
{
    public const string AppName = "DroidIDE";
    public const string AppVersion = "1.0.0";

    /// <summary>Default theme to apply on first launch.</summary>
    public const string DefaultTheme = "Dark";

    /// <summary>Maximum number of editor tabs allowed open simultaneously.</summary>
    public const int MaxOpenTabs = 20;

    /// <summary>Maximum lines to keep in a terminal session buffer.</summary>
    public const int TerminalBufferSize = 10_000;

    /// <summary>Debounce interval (ms) for file system watcher events.</summary>
    public const int FileWatcherDebounceMs = 300;

    /// <summary>Auto-save interval (ms). 0 = disabled.</summary>
    public const int AutoSaveIntervalMs = 5_000;
}
