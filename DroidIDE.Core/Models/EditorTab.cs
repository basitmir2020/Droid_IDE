namespace DroidIDE.Core.Models;

/// <summary>
/// Represents an open editor tab.
/// </summary>
public class EditorTab
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FilePath { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Language { get; set; } = "plaintext";
    public bool IsModified { get; set; }
    public bool IsActive { get; set; }
    public int CursorLine { get; set; } = 1;
    public int CursorColumn { get; set; } = 1;
}
