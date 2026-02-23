using DroidIDE.App.ViewModels;
using DroidIDE.Editor.Monaco;

namespace DroidIDE.App.Views.Layout;

/// <summary>
/// Hosts the Monaco Editor view and synchronizes content between
/// the EditorViewModel (active tab) and the WebView-based Monaco editor.
/// </summary>
public partial class EditorHostView : ContentView
{
    private MonacoEditorBridge? _bridge;
    private EditorViewModel? _viewModel;

    public EditorHostView()
    {
        InitializeComponent();
        BindingContextChanged += OnBindingContextChanged;
    }

    /// <summary>
    /// Inject the Monaco bridge for JS interop.
    /// </summary>
    public void SetBridge(MonacoEditorBridge bridge)
    {
        _bridge = bridge;
        MonacoEditor.SetBridge(bridge);

        // Listen for content changes from Monaco → ViewModel
        bridge.ContentChanged += OnMonacoContentChanged;
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (BindingContext is EditorViewModel vm)
        {
            // Unsubscribe from old VM
            if (_viewModel is not null)
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

            _viewModel = vm;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorViewModel.ActiveTab) && _viewModel?.ActiveTab is not null)
        {
            // Push new tab content to Monaco editor
            var tab = _viewModel.ActiveTab;
            var language = DetectMonacoLanguage(tab.FilePath);
            await MonacoEditor.SetContentAsync(tab.Content, language);
        }
    }

    private void OnMonacoContentChanged(string newContent)
    {
        if (_viewModel?.ActiveTab is not null)
        {
            _viewModel.ActiveTab.Content = newContent;
            _viewModel.ActiveTab.IsModified = true;
        }
    }

    /// <summary>
    /// Map file extension to Monaco language identifier.
    /// </summary>
    private static string DetectMonacoLanguage(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".cs" => "csharp",
            ".csproj" or ".sln" or ".xml" or ".xaml" or ".config" or ".props" or ".targets" => "xml",
            ".json" => "json",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".html" or ".htm" => "html",
            ".css" => "css",
            ".md" => "markdown",
            ".py" => "python",
            ".yaml" or ".yml" => "yaml",
            ".sh" or ".bash" => "shell",
            ".sql" => "sql",
            ".txt" or ".log" => "plaintext",
            _ => "plaintext"
        };
    }
}