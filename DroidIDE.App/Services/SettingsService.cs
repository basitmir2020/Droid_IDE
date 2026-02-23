using System.Text.Json;

namespace DroidIDE.App.Services;

/// <summary>
/// Manages application settings and user preferences with JSON file persistence.
/// Settings are stored per-user in the app's sandboxed data directory.
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;
    private Dictionary<string, object?> _settings = new();

    public SettingsService()
    {
        _settingsPath = Path.Combine(
            FileSystem.AppDataDirectory, "settings.json");
        LoadSettings();
    }

    /// <summary>Fired when any setting changes.</summary>
    public event Action<string, object?>? SettingChanged;

    /// <summary>
    /// Get a setting value, returning default if not found.
    /// </summary>
    public T Get<T>(string key, T defaultValue = default!)
    {
        if (_settings.TryGetValue(key, out var value) && value is not null)
        {
            try
            {
                if (value is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText()) ?? defaultValue;
                }
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// Set a setting value and persist.
    /// </summary>
    public void Set<T>(string key, T value)
    {
        _settings[key] = value;
        SaveSettings();
        SettingChanged?.Invoke(key, value);
    }

    /// <summary>
    /// Check if a setting exists.
    /// </summary>
    public bool HasKey(string key) => _settings.ContainsKey(key);

    /// <summary>
    /// Remove a setting.
    /// </summary>
    public void Remove(string key)
    {
        if (_settings.Remove(key))
        {
            SaveSettings();
            SettingChanged?.Invoke(key, null);
        }
    }

    /// <summary>
    /// Get all setting keys.
    /// </summary>
    public IEnumerable<string> GetAllKeys() => _settings.Keys;

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                _settings = JsonSerializer.Deserialize<Dictionary<string, object?>>(json)
                    ?? new Dictionary<string, object?>();
            }
        }
        catch
        {
            _settings = new Dictionary<string, object?>();
        }
    }

    private void SaveSettings()
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Silently handle write failures
        }
    }
}

/// <summary>
/// Well-known setting keys used throughout the application.
/// </summary>
public static class SettingKeys
{
    public const string Theme = "appearance.theme";
    public const string FontSize = "editor.fontSize";
    public const string FontFamily = "editor.fontFamily";
    public const string TabSize = "editor.tabSize";
    public const string WordWrap = "editor.wordWrap";
    public const string Minimap = "editor.minimap";
    public const string LineNumbers = "editor.lineNumbers";
    public const string AutoSave = "editor.autoSave";
    public const string AutoSaveDelay = "editor.autoSaveDelay";
    public const string RecentProjects = "app.recentProjects";
    public const string LastOpenedProject = "app.lastOpenedProject";
    public const string ExplorerWidth = "layout.explorerWidth";
    public const string BottomPanelHeight = "layout.bottomPanelHeight";
}
