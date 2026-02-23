using System.Text.Json;

namespace DroidIDE.App.Services;

/// <summary>
/// Persists and restores application state (open tabs, panel visibility,
/// recent projects) across sessions using JSON file storage.
/// </summary>
public class AppStateService
{
    private readonly string _statePath;

    public AppStateService()
    {
        _statePath = Path.Combine(
            FileSystem.AppDataDirectory, "app_state.json");
    }

    /// <summary>
    /// Save the current application state.
    /// </summary>
    public async Task SaveStateAsync(AppState state)
    {
        try
        {
            var directory = Path.GetDirectoryName(_statePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_statePath, json);
        }
        catch
        {
            // Silently handle write failures
        }
    }

    /// <summary>
    /// Restore the previously saved application state.
    /// </summary>
    public async Task<AppState> LoadStateAsync()
    {
        try
        {
            if (File.Exists(_statePath))
            {
                var json = await File.ReadAllTextAsync(_statePath);
                return JsonSerializer.Deserialize<AppState>(json) ?? new AppState();
            }
        }
        catch
        {
            // Return default state on failure
        }

        return new AppState();
    }

    /// <summary>
    /// Add a project to the recent projects list.
    /// </summary>
    public async Task AddRecentProjectAsync(string projectPath)
    {
        var state = await LoadStateAsync();

        // Remove if already exists, then add to top
        state.RecentProjects.Remove(projectPath);
        state.RecentProjects.Insert(0, projectPath);

        // Keep only last 10
        if (state.RecentProjects.Count > 10)
            state.RecentProjects = state.RecentProjects.Take(10).ToList();

        await SaveStateAsync(state);
    }

    /// <summary>
    /// Clear all persisted state.
    /// </summary>
    public void ClearState()
    {
        try
        {
            if (File.Exists(_statePath))
                File.Delete(_statePath);
        }
        catch { }
    }
}

/// <summary>
/// Serializable application state persisted between sessions.
/// </summary>
public class AppState
{
    public List<string> OpenTabPaths { get; set; } = [];
    public string? ActiveTabPath { get; set; }
    public string? LastOpenedProjectPath { get; set; }
    public List<string> RecentProjects { get; set; } = [];
    public bool IsExplorerVisible { get; set; } = true;
    public bool IsBottomPanelVisible { get; set; } = true;
    public string ActivePanel { get; set; } = "Explorer";
    public double ExplorerWidth { get; set; } = 250;
    public double BottomPanelHeight { get; set; } = 200;
    public string Theme { get; set; } = "dark";
}
