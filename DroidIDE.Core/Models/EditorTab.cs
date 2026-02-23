namespace DroidIDE.Core.Models;

/// <summary>
/// Represents an open editor tab containing a file's content and cursor state.
/// Managed by the EditorViewModel for tab lifecycle operations (open, close, switch, save).
/// </summary>
public class EditorTab
{
    /// <summary>Gets or sets the unique identifier for this tab instance.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the absolute path to the file this tab represents.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name shown on the tab header (typically the file name).</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the current text content of the file.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets the Monaco language identifier for syntax highlighting (e.g., "csharp", "json").</summary>
    public string Language { get; set; } = "plaintext";

    /// <summary>Gets or sets a value indicating whether the file has unsaved changes.</summary>
    public bool IsModified { get; set; }

    /// <summary>Gets or sets a value indicating whether this tab is the currently active/visible tab.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the 1-based line number of the cursor position.</summary>
    public int CursorLine { get; set; } = 1;

    /// <summary>Gets or sets the 1-based column number of the cursor position.</summary>
    public int CursorColumn { get; set; } = 1;
}
