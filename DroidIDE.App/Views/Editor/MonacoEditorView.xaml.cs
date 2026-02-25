using DroidIDE.Editor.Monaco;

namespace DroidIDE.App.Views.Editor;

/// <summary>
/// Monaco editor view hosting a WebView with the Monaco editor.
/// Bridges C# ViewModel data to/from the JS-based Monaco editor.
/// Intercepts droidide:// URL-scheme navigations fired by JS to receive
/// content-change, cursor-change, and editor-ready signals without
/// requiring a dedicated JavaScript-to-C# bridge API.
/// </summary>
public partial class MonacoEditorView : ContentView, IMonacoWebView
{
    private MonacoEditorBridge? _bridge;
    private bool _isEditorReady;

    public MonacoEditorView()
    {
        InitializeComponent();

        // Intercept page-load result to detect HTML navigation success
        EditorWebView.Navigated += OnWebViewNavigated;

        // Intercept droidide:// URL scheme fired from JS for event callbacks
        EditorWebView.Navigating += OnWebViewNavigating;
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

    /// <summary>
    /// Fired when the HTML page finishes loading.
    /// Note: Monaco itself isn't ready until 'droidide://editorReady' is received.
    /// </summary>
    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        // Page loaded — Monaco will signal its own readiness via editorReady
        if (e.Result != WebNavigationResult.Success)
        {
            System.Diagnostics.Debug.WriteLine($"[MonacoEditorView] WebView navigation failed: {e.Url}");
        }
    }

    /// <summary>
    /// Intercepts droidide:// scheme navigations fired by JS to receive editor events.
    /// Cancels the navigation so the WebView doesn't attempt to actually navigate.
    /// </summary>
    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith("droidide://", StringComparison.OrdinalIgnoreCase))
            return;

        // Cancel the fake navigation — it's just a signal from JS
        e.Cancel = true;

        var uri = new Uri(e.Url);
        var host = uri.Host.ToLowerInvariant();

        switch (host)
        {
            case "editorready":
                _isEditorReady = true;
                System.Diagnostics.Debug.WriteLine("[MonacoEditorView] Monaco editor is ready.");
                break;

            case "contentchanged":
                await HandleContentChangedAsync();
                break;

            case "cursorchanged":
                HandleCursorChanged(uri);
                break;

            case "completionrequested":
                await HandleCompletionRequestedAsync(uri);
                break;
        }
    }

    /// <summary>
    /// Fetches the current editor content from JS and forwards to the bridge.
    /// </summary>
    private async Task HandleContentChangedAsync()
    {
        if (_bridge is null || !_isEditorReady) return;

        try
        {
            var content = await EvaluateJavaScriptAsync("getEditorContent()");
            if (content is not null)
            {
                // Remove surrounding JSON-string quotes that EvaluateJavaScriptAsync may add
                content = content.Trim('"').Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
                _bridge.OnContentChanged(content);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MonacoEditorView] content change error: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses cursor position from the URL query string and forwards to the bridge.
    /// </summary>
    private void HandleCursorChanged(Uri uri)
    {
        if (_bridge is null) return;

        try
        {
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (int.TryParse(query["line"], out var line) &&
                int.TryParse(query["col"], out var col))
            {
                _bridge.OnCursorPositionChanged(line, col);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MonacoEditorView] cursor change error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles a completion request from JS (triggered by trigger character).
    /// Raises an event so the host (EditorHostView) can fetch Roslyn completions.
    /// </summary>
    private async Task HandleCompletionRequestedAsync(Uri uri)
    {
        if (_bridge is null || !_isEditorReady) return;

        try
        {
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (int.TryParse(query["pos"], out var position))
            {
                CompletionRequested?.Invoke(position);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MonacoEditorView] completion request error: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Raised when the user triggers IntelliSense in Monaco.
    /// The host is responsible for fetching completions and pushing them back.
    /// </summary>
    public event Action<int>? CompletionRequested;
}