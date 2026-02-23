namespace DroidIDE.App.Services;

/// <summary>
/// Theme service — manages switching between Dark/Light application themes at runtime.
/// Swaps resource dictionaries and persists the user's preference.
/// </summary>
public class ThemeService
{
    private readonly SettingsService _settings;
    private readonly AppLogger _logger;

    public ThemeService(SettingsService settings, AppLogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>Current active theme name.</summary>
    public string CurrentTheme { get; private set; } = "Dark";

    /// <summary>Fired when the theme changes.</summary>
    public event Action<string>? ThemeChanged;

    /// <summary>
    /// Apply the theme saved in settings (call at app startup).
    /// </summary>
    public void ApplySavedTheme()
    {
        var saved = _settings.Get(SettingKeys.Theme, "Dark");
        SetTheme(saved);
    }

    /// <summary>
    /// Switch to the specified theme ("Dark" or "Light").
    /// </summary>
    public void SetTheme(string theme)
    {
        if (Application.Current is null) return;

        try
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

            // Remove current theme dictionary (it's always the last merged one)
            var existingTheme = mergedDictionaries
                .FirstOrDefault(d => IsThemeDictionary(d));
            if (existingTheme is not null)
                mergedDictionaries.Remove(existingTheme);

            // Load new theme
            ResourceDictionary newTheme = theme == "Light"
                ? new Resources.Themes.LightTheme()
                : new Resources.Themes.DarkTheme();

            mergedDictionaries.Add(newTheme);

            CurrentTheme = theme;
            _settings.Set(SettingKeys.Theme, theme);
            ThemeChanged?.Invoke(theme);
            _logger.LogInfo($"Theme switched to: {theme}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to switch theme", ex);
        }
    }

    /// <summary>Toggle between Dark and Light themes.</summary>
    public void ToggleTheme()
    {
        SetTheme(CurrentTheme == "Dark" ? "Light" : "Dark");
    }

    private static bool IsThemeDictionary(ResourceDictionary dict)
    {
        // Theme dictionaries define the BackgroundPrimary key
        return dict.ContainsKey("BackgroundPrimary");
    }
}
