using System.Text.Json;

namespace DroidIDE.Editor.Monaco;

/// <summary>
/// JavaScript interop bridge for communicating between C# and the Monaco editor
/// running inside a MAUI <see cref="IMonacoWebView"/>. Provides typed methods for
/// content management, theme switching, diagnostic markers, and IntelliSense completions.
/// </summary>
/// <remarks>
/// Uses a <see cref="WeakReference{T}"/> to the WebView to prevent memory leaks when the view is disposed.
/// All JavaScript calls are routed through the private <see cref="EvalAsync"/> helper.
/// </remarks>
public class MonacoEditorBridge
{
    private WeakReference<IMonacoWebView>? _webViewRef;

    /// <summary>Raised when the user types in the editor, providing the full updated content.</summary>
    public event Action<string>? ContentChanged;

    /// <summary>Raised when the cursor position changes; parameters are (line, column), both 1-based.</summary>
    public event Action<int, int>? CursorPositionChanged;

    /// <summary>
    /// Attaches the bridge to a WebView that hosts the Monaco editor instance.
    /// </summary>
    /// <param name="webView">The WebView abstraction to interact with.</param>
    public void AttachWebView(IMonacoWebView webView)
    {
        _webViewRef = new WeakReference<IMonacoWebView>(webView);
    }

    /// <summary>
    /// Sets the editor content and syntax-highlighting language.
    /// </summary>
    /// <param name="content">The full file content to display.</param>
    /// <param name="language">The Monaco language identifier (e.g., "csharp", "json").</param>
    public async Task SetContentAsync(string content, string language)
    {
        var escaped = JsonSerializer.Serialize(content);
        await EvalAsync($"setEditorContent({escaped}, '{language}')");
    }

    /// <summary>
    /// Retrieves the current editor content as a string.
    /// </summary>
    /// <returns>The current text content, or an empty string if unavailable.</returns>
    public async Task<string> GetContentAsync()
    {
        var result = await EvalAsync("getEditorContent()");
        return result ?? string.Empty;
    }

    /// <summary>
    /// Changes the editor syntax-highlighting language mode.
    /// </summary>
    /// <param name="language">The Monaco language identifier (e.g., "csharp", "xml", "json").</param>
    public async Task SetLanguageAsync(string language)
    {
        await EvalAsync($"setEditorLanguage('{language}')");
    }

    /// <summary>
    /// Sets the editor color theme.
    /// </summary>
    /// <param name="theme">The Monaco theme name: "vs-dark", "vs", or "hc-black".</param>
    public async Task SetThemeAsync(string theme = "vs-dark")
    {
        await EvalAsync($"setEditorTheme('{theme}')");
    }

    /// <summary>
    /// Pushes diagnostic markers (squiggly underlines) to the editor overlay.
    /// </summary>
    /// <param name="markers">The collection of <see cref="MonacoMarker"/> objects to display.</param>
    public async Task SetMarkersAsync(IEnumerable<MonacoMarker> markers)
    {
        var json = JsonSerializer.Serialize(markers);
        await EvalAsync($"setEditorMarkers({json})");
    }

    /// <summary>
    /// Gets the current cursor position in the editor.
    /// </summary>
    /// <returns>A tuple of (Line, Column), both 1-based. Defaults to (1, 1) on error.</returns>
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
    /// Provides completion items to the Monaco autocomplete widget.
    /// </summary>
    /// <param name="completions">The collection of <see cref="MonacoCompletionItem"/> entries to show.</param>
    public async Task SetCompletionsAsync(IEnumerable<MonacoCompletionItem> completions)
    {
        var json = JsonSerializer.Serialize(completions);
        await EvalAsync($"setCompletions({json})");
    }

    /// <summary>
    /// Toggles the editor between read-only and editable mode.
    /// </summary>
    /// <param name="readOnly"><c>true</c> to make the editor read-only; <c>false</c> for editable.</param>
    public async Task SetReadOnlyAsync(bool readOnly)
    {
        await EvalAsync($"setReadOnly({(readOnly ? "true" : "false")})");
    }

    /// <summary>
    /// Navigates the editor cursor to a specific line and column and scrolls the line into view.
    /// </summary>
    /// <param name="line">The 1-based target line number.</param>
    /// <param name="column">The 1-based target column number.</param>
    public async Task GoToPositionAsync(int line, int column)
    {
        await EvalAsync($"goToPosition({line}, {column})");
    }

    /// <summary>
    /// Callback invoked from JavaScript when the editor content changes due to user input.
    /// </summary>
    /// <param name="newContent">The full updated editor content.</param>
    public void OnContentChanged(string newContent)
    {
        ContentChanged?.Invoke(newContent);
    }

    /// <summary>
    /// Callback invoked from JavaScript when the cursor position changes.
    /// </summary>
    /// <param name="line">The 1-based line number of the new cursor position.</param>
    /// <param name="column">The 1-based column number of the new cursor position.</param>
    public void OnCursorPositionChanged(int line, int column)
    {
        CursorPositionChanged?.Invoke(line, column);
    }

    /// <summary>
    /// Evaluates a JavaScript expression in the attached WebView.
    /// </summary>
    /// <param name="js">The JavaScript code to execute.</param>
    /// <returns>The evaluation result string, or <c>null</c> if the WebView is unavailable.</returns>
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
/// Abstraction for the MAUI WebView that hosts the Monaco editor.
/// Enables JavaScript evaluation without direct WebView coupling.
/// </summary>
public interface IMonacoWebView
{
    /// <summary>Evaluates a JavaScript expression and returns the result as a string.</summary>
    /// <param name="js">The JavaScript code to evaluate.</param>
    /// <returns>The result of the evaluation, or <c>null</c>.</returns>
    Task<string?> EvaluateJavaScriptAsync(string js);
}

/// <summary>
/// Represents a diagnostic marker (squiggly underline) to display in the Monaco editor.
/// Maps directly to the Monaco <c>IMarkerData</c> interface.
/// </summary>
public class MonacoMarker
{
    /// <summary>Gets or sets the 1-based start line of the marker span.</summary>
    public int StartLineNumber { get; set; }

    /// <summary>Gets or sets the 1-based start column of the marker span.</summary>
    public int StartColumn { get; set; }

    /// <summary>Gets or sets the 1-based end line of the marker span.</summary>
    public int EndLineNumber { get; set; }

    /// <summary>Gets or sets the 1-based end column of the marker span.</summary>
    public int EndColumn { get; set; }

    /// <summary>Gets or sets the diagnostic message displayed on hover.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the marker severity. Monaco values: 1=Hint, 2=Info, 4=Warning, 8=Error.</summary>
    public int Severity { get; set; }
}

/// <summary>
/// Represents a completion item for the Monaco autocomplete widget.
/// Maps to the Monaco <c>CompletionItem</c> interface.
/// </summary>
public class MonacoCompletionItem
{
    /// <summary>Gets or sets the display label shown in the autocomplete list.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the Monaco CompletionItemKind enum value (0=Method, 5=Class, etc.).</summary>
    public int Kind { get; set; }

    /// <summary>Gets or sets the text to insert when the completion is accepted.</summary>
    public string InsertText { get; set; } = string.Empty;

    /// <summary>Gets or sets additional detail text shown beside the completion label.</summary>
    public string? Detail { get; set; }

    /// <summary>Gets or sets the documentation string shown in the completion detail panel.</summary>
    public string? Documentation { get; set; }
}

/// <summary>
/// Internal DTO for deserializing cursor position JSON from the Monaco editor JavaScript callback.
/// </summary>
internal class CursorPosition
{
    /// <summary>Gets or sets the 1-based line number.</summary>
    public int Line { get; set; }

    /// <summary>Gets or sets the 1-based column number.</summary>
    public int Column { get; set; }
}
