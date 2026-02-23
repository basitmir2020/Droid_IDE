using System.Text.Json;

namespace DroidIDE.Editor.Monaco;

/// <summary>
/// JS interop bridge for communicating between C# and the Monaco editor
/// running inside a MAUI WebView. Provides typed methods for all editor operations.
/// </summary>
public class MonacoEditorBridge
{
    private WeakReference<IMonacoWebView>? _webViewRef;

    /// <summary>Fired when editor content changes (from user typing).</summary>
    public event Action<string>? ContentChanged;

    /// <summary>Fired when cursor position changes.</summary>
    public event Action<int, int>? CursorPositionChanged;

    /// <summary>
    /// Attach to a WebView that hosts the Monaco editor.
    /// </summary>
    public void AttachWebView(IMonacoWebView webView)
    {
        _webViewRef = new WeakReference<IMonacoWebView>(webView);
    }

    /// <summary>
    /// Set the editor content and language.
    /// </summary>
    public async Task SetContentAsync(string content, string language)
    {
        var escaped = JsonSerializer.Serialize(content);
        await EvalAsync($"setEditorContent({escaped}, '{language}')");
    }

    /// <summary>
    /// Get the current editor content.
    /// </summary>
    public async Task<string> GetContentAsync()
    {
        var result = await EvalAsync("getEditorContent()");
        return result ?? string.Empty;
    }

    /// <summary>
    /// Change the editor language mode (syntax highlighting).
    /// </summary>
    public async Task SetLanguageAsync(string language)
    {
        await EvalAsync($"setEditorLanguage('{language}')");
    }

    /// <summary>
    /// Set the editor theme (e.g., "vs-dark", "vs", "hc-black").
    /// </summary>
    public async Task SetThemeAsync(string theme = "vs-dark")
    {
        await EvalAsync($"setEditorTheme('{theme}')");
    }

    /// <summary>
    /// Push diagnostic markers (squiggly underlines) to the editor.
    /// </summary>
    public async Task SetMarkersAsync(IEnumerable<MonacoMarker> markers)
    {
        var json = JsonSerializer.Serialize(markers);
        await EvalAsync($"setEditorMarkers({json})");
    }

    /// <summary>
    /// Get current cursor position (line, column).
    /// </summary>
    public async Task<(int Line, int Column)> GetCursorPositionAsync()
    {
        var result = await EvalAsync("getCursorPosition()");
        if (string.IsNullOrEmpty(result)) return (1, 1);

        try
        {
            var pos = JsonSerializer.Deserialize<CursorPosition>(result);
            return (pos?.Line ?? 1, pos?.Column ?? 1);
        }
        catch
        {
            return (1, 1);
        }
    }

    /// <summary>
    /// Set completions to show in the editor autocomplete widget.
    /// </summary>
    public async Task SetCompletionsAsync(IEnumerable<MonacoCompletionItem> completions)
    {
        var json = JsonSerializer.Serialize(completions);
        await EvalAsync($"setCompletions({json})");
    }

    /// <summary>
    /// Set the editor to read-only or editable mode.
    /// </summary>
    public async Task SetReadOnlyAsync(bool readOnly)
    {
        await EvalAsync($"setReadOnly({(readOnly ? "true" : "false")})");
    }

    /// <summary>
    /// Navigate to a specific line and column.
    /// </summary>
    public async Task GoToPositionAsync(int line, int column)
    {
        await EvalAsync($"goToPosition({line}, {column})");
    }

    /// <summary>
    /// Called from JS when content changes.
    /// </summary>
    public void OnContentChanged(string newContent)
    {
        ContentChanged?.Invoke(newContent);
    }

    /// <summary>
    /// Called from JS when cursor position changes.
    /// </summary>
    public void OnCursorPositionChanged(int line, int column)
    {
        CursorPositionChanged?.Invoke(line, column);
    }

    private async Task<string?> EvalAsync(string js)
    {
        if (_webViewRef?.TryGetTarget(out var webView) == true)
        {
            return await webView.EvaluateJavaScriptAsync(js);
        }
        return null;
    }
}

/// <summary>
/// Abstraction for the WebView that hosts Monaco.
/// </summary>
public interface IMonacoWebView
{
    Task<string?> EvaluateJavaScriptAsync(string js);
}

/// <summary>
/// Represents a diagnostic marker in Monaco editor.
/// </summary>
public class MonacoMarker
{
    public int StartLineNumber { get; set; }
    public int StartColumn { get; set; }
    public int EndLineNumber { get; set; }
    public int EndColumn { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Severity { get; set; } // 1=Hint, 2=Info, 4=Warning, 8=Error
}

/// <summary>
/// Represents a completion item for Monaco autocomplete.
/// </summary>
public class MonacoCompletionItem
{
    public string Label { get; set; } = string.Empty;
    public int Kind { get; set; } // Monaco CompletionItemKind enum values
    public string InsertText { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public string? Documentation { get; set; }
}

/// <summary>
/// Internal cursor position DTO.
/// </summary>
internal class CursorPosition
{
    public int Line { get; set; }
    public int Column { get; set; }
}
