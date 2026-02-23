using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// High-level editor service for opening, saving, and managing file content in the editor.
/// Implemented by <c>EditorService</c> in the App layer.
/// </summary>
public interface IEditorService
{
    /// <summary>Opens a file from disk and creates an <see cref="EditorTab"/> with its content and detected language.</summary>
    /// <param name="filePath">The absolute path to the file to open.</param>
    /// <returns>An <see cref="EditorTab"/> populated with the file's content, name, and language.</returns>
    Task<EditorTab> OpenFileAsync(string filePath);

    /// <summary>Saves the current content of an editor tab back to disk.</summary>
    /// <param name="tab">The <see cref="EditorTab"/> containing the file path and content to save.</param>
    Task SaveFileAsync(EditorTab tab);

    /// <summary>Detects the Monaco editor language identifier from a file's extension.</summary>
    /// <param name="filePath">The file path to analyze (only the extension is used).</param>
    /// <returns>A Monaco language identifier string (e.g., "csharp", "json", "xml").</returns>
    string DetectLanguage(string filePath);
}
