using DroidIDE.Editor.Monaco;

namespace DroidIDE.App.Views.Editor;

/// <summary>
/// Monaco editor view hosting a WebView with the Monaco editor.
/// Bridges C# ViewModel data to/from the JS-based Monaco editor.
/// </summary>
public partial class MonacoEditorView : ContentView, IMonacoWebView
{
    private MonacoEditorBridge? _bridge;
    private bool _isEditorReady;

    public MonacoEditorView()
    {
        InitializeComponent();
        EditorWebView.Navigated += OnWebViewNavigated;
    }

    /// <summary>
    /// Set the bridge instance for JS interop.
    /// </summary>
    public void SetBridge(MonacoEditorBridge bridge)
    {
        _bridge = bridge;
        _bridge.AttachWebView(this);
    }

    /// <summary>
    /// Load content into the Monaco editor.
    /// </summary>
    public async Task SetContentAsync(string content, string language)
    {
        if (_bridge is not null && _isEditorReady)
        {
            await _bridge.SetContentAsync(content, language);
        }
    }

    /// <summary>
    /// Get the current content from the Monaco editor.
    /// </summary>
    public async Task<string> GetContentAsync()
    {
        if (_bridge is not null && _isEditorReady)
        {
            return await _bridge.GetContentAsync();
        }
        return string.Empty;
    }

    /// <summary>
    /// Push diagnostic markers to the Monaco editor.
    /// </summary>
    public async Task SetMarkersAsync(IEnumerable<MonacoMarker> markers)
    {
        if (_bridge is not null && _isEditorReady)
        {
            await _bridge.SetMarkersAsync(markers);
        }
    }

    /// <summary>
    /// Implements IMonacoWebView for the bridge.
    /// </summary>
    public async Task<string?> EvaluateJavaScriptAsync(string js)
    {
        try
        {
            return await EditorWebView.EvaluateJavaScriptAsync(js);
        }
        catch
        {
            return null;
        }
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        _isEditorReady = e.Result == WebNavigationResult.Success;
    }
}