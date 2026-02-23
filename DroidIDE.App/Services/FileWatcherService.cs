namespace DroidIDE.App.Services;

/// <summary>
/// Monitors the file system for changes and emits events for live reload.
/// Watches for file create, modify, delete, and rename operations.
/// </summary>
public class FileWatcherService : IDisposable
{
    private FileSystemWatcher? _watcher;
    private string? _watchedPath;

    /// <summary>Fired when a file is created.</summary>
    public event Action<string>? FileCreated;

    /// <summary>Fired when a file is modified.</summary>
    public event Action<string>? FileChanged;

    /// <summary>Fired when a file is deleted.</summary>
    public event Action<string>? FileDeleted;

    /// <summary>Fired when a file is renamed.</summary>
    public event Action<string, string>? FileRenamed;

    /// <summary>Whether the watcher is currently active.</summary>
    public bool IsWatching => _watcher?.EnableRaisingEvents ?? false;

    /// <summary>
    /// Start watching a directory for file changes.
    /// </summary>
    public void StartWatching(string directoryPath)
    {
        StopWatching();

        if (!Directory.Exists(directoryPath)) return;

        _watchedPath = directoryPath;
        _watcher = new FileSystemWatcher(directoryPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
                | NotifyFilters.DirectoryName
                | NotifyFilters.LastWrite
                | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        _watcher.Created += (_, e) =>
        {
            if (!ShouldIgnore(e.FullPath))
                FileCreated?.Invoke(e.FullPath);
        };

        _watcher.Changed += (_, e) =>
        {
            if (!ShouldIgnore(e.FullPath))
                FileChanged?.Invoke(e.FullPath);
        };

        _watcher.Deleted += (_, e) =>
        {
            if (!ShouldIgnore(e.FullPath))
                FileDeleted?.Invoke(e.FullPath);
        };

        _watcher.Renamed += (_, e) =>
        {
            if (!ShouldIgnore(e.FullPath))
                FileRenamed?.Invoke(e.OldFullPath, e.FullPath);
        };
    }

    /// <summary>
    /// Stop watching for file changes.
    /// </summary>
    public void StopWatching()
    {
        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
        }
        _watchedPath = null;
    }

    /// <summary>
    /// Ignore changes in generated/build directories.
    /// </summary>
    private bool ShouldIgnore(string path)
    {
        if (_watchedPath is null) return true;

        var relative = Path.GetRelativePath(_watchedPath, path);
        var parts = relative.Split(Path.DirectorySeparatorChar);
        var ignoredDirs = new[] { "bin", "obj", ".git", ".vs", "node_modules" };
        return parts.Any(p => ignoredDirs.Contains(p, StringComparer.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        StopWatching();
        GC.SuppressFinalize(this);
    }
}
