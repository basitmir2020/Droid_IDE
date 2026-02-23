using DroidIDE.App.Services;

namespace DroidIDE.App.Views.Layout;

/// <summary>
/// Welcome/onboarding view — shown on first launch. Provides quick actions
/// and shortcuts reference. Can be dismissed permanently.
/// </summary>
public partial class WelcomeView : ContentView
{
    private SettingsService? _settings;

    /// <summary>Fired when user wants to open a folder.</summary>
    public event Action? OpenFolderRequested;

    /// <summary>Fired when user wants to clone a repository.</summary>
    public event Action? CloneRepoRequested;

    /// <summary>Fired when user wants to create a new project.</summary>
    public event Action? NewProjectRequested;

    public WelcomeView()
    {
        InitializeComponent();
    }

    /// <summary>Inject settings service for "don't show again" persistence.</summary>
    public void SetSettings(SettingsService settings)
    {
        _settings = settings;
    }

    /// <summary>Returns true if the welcome view should be shown.</summary>
    public static bool ShouldShow(SettingsService settings)
    {
        return !settings.Get("welcome.dontShowAgain", false);
    }

    private void OnOpenFolderTapped(object? sender, EventArgs e)
    {
        OpenFolderRequested?.Invoke();
    }

    private void OnCloneRepoTapped(object? sender, EventArgs e)
    {
        CloneRepoRequested?.Invoke();
    }

    private void OnNewProjectTapped(object? sender, EventArgs e)
    {
        NewProjectRequested?.Invoke();
    }

    private void OnDontShowToggle(object? sender, EventArgs e)
    {
        DontShowAgainCheckbox.IsChecked = !DontShowAgainCheckbox.IsChecked;
        _settings?.Set("welcome.dontShowAgain", DontShowAgainCheckbox.IsChecked);
    }
}
