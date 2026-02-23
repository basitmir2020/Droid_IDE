using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// High-level editor service for managing open files, tabs, and code operations.
/// </summary>
public interface IEditorService
{
    /// <summary>Open a file and return its content along with detected language.</summary>
    Task<EditorTab> OpenFileAsync(string filePath);

    /// <summary>Save the content of a tab back to disk.</summary>
    Task SaveFileAsync(EditorTab tab);

    /// <summary>Detect the Monaco language identifier from a file extension.</summary>
    string DetectLanguage(string filePath);
}
