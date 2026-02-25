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

    private CancellationTokenSource? _diagnosticsCts;

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
            // If the UI (like CollectionView) tries to set null when we have tabs, 
            // ignore it to prevent race conditions during list refreshes.
            if (value is null && OpenTabs.Count > 0) return;

            // Deactivate previous tab
            if (_activeTab is not null)
                _activeTab.IsActive = false;

            if (SetProperty(ref _activeTab, value) && value is not null)
            {
                value.IsActive = true;

                // Sync to Monaco and Roslyn
                RefreshActiveEditor();
                
                // Fire property changed for nested properties just in case
                OnPropertyChanged(nameof(ActiveTab));
            }
        }
    }

    private void RefreshActiveEditor()
    {
        if (ActiveTab is null) return;

        // Update Roslyn workspace with the new tab's content
        _ = Task.Run(() => _languageService.UpdateDocument(ActiveTab.FilePath, ActiveTab.Content));

        // Trigger diagnostics for C# files (other files: clear markers)
        if (ActiveTab.Language == "csharp")
            _ = RunDiagnosticsAsync(ActiveTab);
        else
            _ = _bridge.SetMarkersAsync([]);
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

    /// <summary>
    /// Finds the source definition for the symbol at the given offset and navigates to it.
    /// </summary>
    public async Task GoToDefinitionAsync(int caretOffset)
    {
        if (ActiveTab is null || ActiveTab.Language != "csharp") return;

        try
        {
            var def = await _languageService.GetDefinitionAsync(ActiveTab.FilePath, caretOffset);
            if (def is null) return;

            // If it's a different file, open it first
            if (def.FilePath != ActiveTab.FilePath)
            {
                var tab = await OpenFileForEditorAsync(def.FilePath);
                if (tab is null) return;
            }

            // Navigate to the line/column in the (now active) editor
            await _bridge.GoToPositionAsync(def.StartLine, def.StartColumn);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditorViewModel] go to definition error: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates to a specific line and column in the active editor.
    /// </summary>
    public async Task GoToLineAsync(int line, int column = 1)
    {
        if (_bridge is null) return;
        await _bridge.GoToPositionAsync(line, column);
    }

    /// <summary>
    /// Placeholder for reference finding logic.
    /// </summary>
    public async Task FindReferencesAsync(int caretOffset)
    {
        // TODO: Implement reference tool window or global search integration
        await Task.CompletedTask;
    }

    // ── Private helpers ──────────────────────────────────────────

    /// <summary>
    /// Runs Roslyn diagnostics for a C# tab on a background thread.
    /// Cancels any existing diagnostic tasks for this editor.
    /// </summary>
    private async Task RunDiagnosticsAsync(EditorTab tab)
    {
        // Cancel existing diagnostics task
        _diagnosticsCts?.Cancel();
        _diagnosticsCts = new CancellationTokenSource();
        var token = _diagnosticsCts.Token;

        try
        {
            // Optional: short internal debounce to avoid overwhelming on very fast bridge updates
            await Task.Delay(100, token);

            // AnalyzeAsync fires DiagnosticsUpdated when complete
            await _diagnosticService.AnalyzeAsync(tab.FilePath, tab.Content, token);
        }
        catch (TaskCanceledException)
        {
            // Normal when typing fast
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
