using System.Collections.ObjectModel;
using System.Windows.Input;
using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.App.ViewModels;

/// <summary>
/// ViewModel for the editor area — manages open tabs, active tab, and file saving.
/// </summary>
public class EditorViewModel : BaseViewModel
{
    private readonly IEditorService _editorService;

    public ObservableCollection<EditorTab> OpenTabs { get; } = [];

    private EditorTab? _activeTab;
    public EditorTab? ActiveTab
    {
        get => _activeTab;
        set
        {
            // Deactivate previous tab
            if (_activeTab is not null)
                _activeTab.IsActive = false;

            if (SetProperty(ref _activeTab, value) && value is not null)
                value.IsActive = true;
        }
    }

    public ICommand CloseTabCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAllCommand { get; }

    public EditorViewModel(IEditorService editorService)
    {
        _editorService = editorService;
        Title = "Editor";

        CloseTabCommand = new RelayCommand(param => CloseTab(param as EditorTab));
        SaveCommand = new AsyncRelayCommand(SaveActiveTabAsync);
        SaveAllCommand = new AsyncRelayCommand(SaveAllTabsAsync);
    }

    /// <summary>
    /// Opens a file in a new tab, or activates an existing tab if already open.
    /// </summary>
    public void OpenTab(EditorTab tab)
    {
        // Check if this file is already open
        var existing = OpenTabs.FirstOrDefault(t => t.FilePath == tab.FilePath);
        if (existing is not null)
        {
            ActiveTab = existing;
            return;
        }

        OpenTabs.Add(tab);
        ActiveTab = tab;
    }

    /// <summary>
    /// Closes a tab. Prompts save if modified (TODO: add save prompt).
    /// </summary>
    public void CloseTab(EditorTab? tab)
    {
        if (tab is null) return;

        var index = OpenTabs.IndexOf(tab);
        OpenTabs.Remove(tab);

        // Activate adjacent tab if the closed tab was active
        if (tab == ActiveTab && OpenTabs.Count > 0)
        {
            ActiveTab = OpenTabs[Math.Min(index, OpenTabs.Count - 1)];
        }
        else if (OpenTabs.Count == 0)
        {
            ActiveTab = null;
        }
    }

    /// <summary>
    /// Saves the currently active tab.
    /// </summary>
    public async Task SaveActiveTabAsync()
    {
        if (ActiveTab is null || !ActiveTab.IsModified) return;

        await _editorService.SaveFileAsync(ActiveTab);
        ActiveTab.IsModified = false;
        OnPropertyChanged(nameof(ActiveTab));
    }

    /// <summary>
    /// Saves all modified tabs.
    /// </summary>
    public async Task SaveAllTabsAsync()
    {
        foreach (var tab in OpenTabs.Where(t => t.IsModified))
        {
            await _editorService.SaveFileAsync(tab);
            tab.IsModified = false;
        }
    }
}
