using System.Collections.ObjectModel;
using System.Windows.Input;
using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;
using DroidIDE.Editor.Diagnostics;
using DroidIDE.Editor.LanguageServer;
using DroidIDE.Editor.Monaco;

namespace DroidIDE.App.ViewModels;

/// <summary>
/// ViewModel for the editor area — manages open tabs, active tab, file saving,
/// Roslyn diagnostics (squiggles), and IntelliSense completions.
/// </summary>
public class EditorViewModel : BaseViewModel
{
    private readonly IEditorService _editorService;
    private readonly DiagnosticService _diagnosticService;
    private readonly RoslynLanguageService _languageService;
    private readonly MonacoEditorBridge _bridge;

    /// <summary>Currently open editor tabs.</summary>
    public ObservableCollection<EditorTab> OpenTabs { get; } = [];

    private EditorTab? _activeTab;

    /// <summary>
    /// The currently focused tab. Changing this triggers Roslyn document sync
    /// and pushes content to the Monaco editor via the bridge.
    /// </summary>
    public EditorTab? ActiveTab
    {
        get => _activeTab;
        set
        {
            // Deactivate previous tab
            if (_activeTab is not null)
                _activeTab.IsActive = false;

            if (SetProperty(ref _activeTab, value) && value is not null)
            {
                value.IsActive = true;

                // Update Roslyn workspace with the new tab's content
                _ = Task.Run(() => _languageService.UpdateDocument(value.FilePath, value.Content));

                // Trigger diagnostics for C# files (other files: clear markers)
                if (value.Language == "csharp")
                    _ = RunDiagnosticsAsync(value);
                else
                    _ = _bridge.SetMarkersAsync([]);
            }
        }
    }

    public ICommand CloseTabCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAllCommand { get; }

    public EditorViewModel(
        IEditorService editorService,
        DiagnosticService diagnosticService,
        RoslynLanguageService languageService,
        MonacoEditorBridge bridge)
    {
        _editorService = editorService;
        _diagnosticService = diagnosticService;
        _languageService = languageService;
        _bridge = bridge;
        Title = "Editor";

        CloseTabCommand  = new RelayCommand(param => CloseTab(param as EditorTab));
        SaveCommand      = new AsyncRelayCommand(SaveActiveTabAsync);
        SaveAllCommand   = new AsyncRelayCommand(SaveAllTabsAsync);

        // Wire Roslyn diagnostic results → Monaco squiggle markers
        _diagnosticService.DiagnosticsUpdated += OnDiagnosticsUpdated;

        // Wire bridge content-change events: update tab and re-run diagnostics
        _bridge.ContentChanged += OnBridgeContentChanged;
    }

    // ── Tab management ───────────────────────────────────────────

    /// <summary>
    /// Opens a file in a new tab, or activates an existing tab if already open.
    /// </summary>
    public void OpenTab(EditorTab tab)
    {
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
    /// Opens a file by path (used by Explorer and Search result navigation).
    /// Returns the opened or existing tab.
    /// </summary>
    public async Task<EditorTab?> OpenFileForEditorAsync(string filePath)
    {
        // Return existing tab if already open
        var existing = OpenTabs.FirstOrDefault(t => t.FilePath == filePath);
        if (existing is not null)
        {
            ActiveTab = existing;
            return existing;
        }

        try
        {
            var tab = await _editorService.OpenFileAsync(filePath);
            OpenTab(tab);
            return tab;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditorViewModel] OpenFileForEditorAsync failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Closes a tab. If the tab has unsaved changes they are discarded (save prompt is a future TODO).
    /// </summary>
    public void CloseTab(EditorTab? tab)
    {
        if (tab is null) return;

        var index = OpenTabs.IndexOf(tab);
        OpenTabs.Remove(tab);

        if (tab == ActiveTab && OpenTabs.Count > 0)
            ActiveTab = OpenTabs[Math.Min(index, OpenTabs.Count - 1)];
        else if (OpenTabs.Count == 0)
            ActiveTab = null;
    }

    // ── Save operations ──────────────────────────────────────────

    /// <summary>Saves the currently active tab if it has unsaved changes.</summary>
    public async Task SaveActiveTabAsync()
    {
        if (ActiveTab is null || !ActiveTab.IsModified) return;

        await _editorService.SaveFileAsync(ActiveTab);
        ActiveTab.IsModified = false;
        OnPropertyChanged(nameof(ActiveTab));

        // Re-run diagnostics after save so markers stay up-to-date
        if (ActiveTab.Language == "csharp")
            _ = RunDiagnosticsAsync(ActiveTab);
    }

    /// <summary>Saves all modified open tabs.</summary>
    public async Task SaveAllTabsAsync()
    {
        foreach (var tab in OpenTabs.Where(t => t.IsModified).ToList())
        {
            await _editorService.SaveFileAsync(tab);
            tab.IsModified = false;
        }
    }

    // ── IntelliSense completions ─────────────────────────────────

    /// <summary>
    /// Fetches Roslyn completions at the given character offset and pushes
    /// them to the Monaco editor. Called by MonacoEditorView when JS fires
    /// the droidide://completionRequested URL scheme.
    /// </summary>
    public async Task ProvideCompletionsAsync(int caretOffset)
    {
        if (ActiveTab is null || ActiveTab.Language != "csharp") return;

        try
        {
            var completions = await _languageService.GetCompletionsAsync(
                ActiveTab.FilePath, caretOffset);

            var monacoItems = completions.Select(c => new MonacoCompletionItem
            {
                Label      = c.Label,
                Kind       = (int)c.Kind,
                InsertText = c.InsertText ?? c.Label,
                Detail     = c.FilterText ?? string.Empty
            });

            await _bridge.SetCompletionsAsync(monacoItems);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditorViewModel] completions error: {ex.Message}");
        }
    }

    // ── Private helpers ──────────────────────────────────────────

    /// <summary>
    /// Runs Roslyn diagnostics for a C# tab on a background thread.
    /// </summary>
    private async Task RunDiagnosticsAsync(EditorTab tab)
    {
        try
        {
            // AnalyzeAsync fires DiagnosticsUpdated when complete
            await _diagnosticService.AnalyzeAsync(tab.FilePath, tab.Content);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditorViewModel] diagnostics error: {ex.Message}");
        }
    }

    /// <summary>
    /// Converts DiagnosticItems from Roslyn into MonacoMarkers and pushes them to the editor.
    /// Only applies markers if the active tab matches the file that was analyzed.
    /// </summary>
    private async void OnDiagnosticsUpdated(string filePath, List<DiagnosticItem> diagnostics)
    {
        if (ActiveTab?.FilePath != filePath) return;

        try
        {
            var markers = diagnostics.Select(d => new MonacoMarker
            {
                StartLineNumber = d.Line,
                StartColumn     = d.Column,
                EndLineNumber   = d.EndLine,
                EndColumn       = d.EndColumn,
                Message         = d.Message,
                Severity        = MapSeverity(d.Severity)
            });

            await _bridge.SetMarkersAsync(markers);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditorViewModel] marker push error: {ex.Message}");
        }
    }

    /// <summary>
    /// Called by the bridge when Monaco fires a content-change signal.
    /// Updates the active tab's content and marks it as modified.
    /// </summary>
    private void OnBridgeContentChanged(string newContent)
    {
        if (ActiveTab is null) return;

        ActiveTab.Content = newContent;
        ActiveTab.IsModified = true;

        // Debounce diagnostics — only re-analyze C# content
        if (ActiveTab.Language == "csharp")
            _ = RunDiagnosticsAsync(ActiveTab);
    }

    /// <summary>Maps a Roslyn/Core DiagnosticSeverity to the Monaco marker severity integer.</summary>
    private static int MapSeverity(Core.Enums.DiagnosticSeverity severity) => severity switch
    {
        Core.Enums.DiagnosticSeverity.Error   => 8, // monaco.MarkerSeverity.Error
        Core.Enums.DiagnosticSeverity.Warning => 4, // monaco.MarkerSeverity.Warning
        Core.Enums.DiagnosticSeverity.Info    => 2, // monaco.MarkerSeverity.Info
        _                                     => 1  // monaco.MarkerSeverity.Hint
    };
}
