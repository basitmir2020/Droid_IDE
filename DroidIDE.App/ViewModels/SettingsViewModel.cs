using System.Windows.Input;
using DroidIDE.App.Services;

namespace DroidIDE.App.ViewModels;

/// <summary>
/// ViewModel for the Settings/Preferences panel.
/// </summary>
public class SettingsViewModel : BaseViewModel
{
    private readonly SettingsService _settingsService;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        Title = "Settings";

        SaveCommand = new RelayCommand(Save);
        ResetCommand = new RelayCommand(Reset);

        LoadFromSettings();
    }

    // ── Editor Settings ──

    private int _fontSize = 14;
    public int FontSize
    {
        get => _fontSize;
        set { if (SetProperty(ref _fontSize, value)) _settingsService.Set(SettingKeys.FontSize, value); }
    }

    private string _fontFamily = "Cascadia Code";
    public string FontFamily
    {
        get => _fontFamily;
        set { if (SetProperty(ref _fontFamily, value)) _settingsService.Set(SettingKeys.FontFamily, value); }
    }

    private int _tabSize = 4;
    public int TabSize
    {
        get => _tabSize;
        set { if (SetProperty(ref _tabSize, value)) _settingsService.Set(SettingKeys.TabSize, value); }
    }

    private bool _wordWrap = true;
    public bool WordWrap
    {
        get => _wordWrap;
        set { if (SetProperty(ref _wordWrap, value)) _settingsService.Set(SettingKeys.WordWrap, value); }
    }

    private bool _minimap;
    public bool Minimap
    {
        get => _minimap;
        set { if (SetProperty(ref _minimap, value)) _settingsService.Set(SettingKeys.Minimap, value); }
    }

    private bool _lineNumbers = true;
    public bool LineNumbers
    {
        get => _lineNumbers;
        set { if (SetProperty(ref _lineNumbers, value)) _settingsService.Set(SettingKeys.LineNumbers, value); }
    }

    // ── Auto Save ──

    private bool _autoSave;
    public bool AutoSave
    {
        get => _autoSave;
        set { if (SetProperty(ref _autoSave, value)) _settingsService.Set(SettingKeys.AutoSave, value); }
    }

    private int _autoSaveDelay = 1000;
    public int AutoSaveDelay
    {
        get => _autoSaveDelay;
        set { if (SetProperty(ref _autoSaveDelay, value)) _settingsService.Set(SettingKeys.AutoSaveDelay, value); }
    }

    // ── Appearance ──

    private string _theme = "Dark";
    public string Theme
    {
        get => _theme;
        set { if (SetProperty(ref _theme, value)) _settingsService.Set(SettingKeys.Theme, value); }
    }

    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }

    /// <summary>Fired when settings change and the editor should refresh.</summary>
    public event Action? SettingsApplied;

    private void LoadFromSettings()
    {
        _fontSize = _settingsService.Get(SettingKeys.FontSize, 14);
        _fontFamily = _settingsService.Get(SettingKeys.FontFamily, "Cascadia Code");
        _tabSize = _settingsService.Get(SettingKeys.TabSize, 4);
        _wordWrap = _settingsService.Get(SettingKeys.WordWrap, true);
        _minimap = _settingsService.Get(SettingKeys.Minimap, false);
        _lineNumbers = _settingsService.Get(SettingKeys.LineNumbers, true);
        _autoSave = _settingsService.Get(SettingKeys.AutoSave, false);
        _autoSaveDelay = _settingsService.Get(SettingKeys.AutoSaveDelay, 1000);
        _theme = _settingsService.Get(SettingKeys.Theme, "Dark");
    }

    private void Save()
    {
        SettingsApplied?.Invoke();
    }

    private void Reset()
    {
        FontSize = 14;
        FontFamily = "Cascadia Code";
        TabSize = 4;
        WordWrap = true;
        Minimap = false;
        LineNumbers = true;
        AutoSave = false;
        AutoSaveDelay = 1000;
        Theme = "Dark";
        SettingsApplied?.Invoke();
    }
}
