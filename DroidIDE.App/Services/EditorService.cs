using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.App.Services;

/// <summary>
/// High-level editor service for opening/saving files and detecting languages.
/// </summary>
public class EditorService : IEditorService
{
    private readonly IFileSystemService _fileSystemService;

    public EditorService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    public async Task<EditorTab> OpenFileAsync(string filePath)
    {
        var content = await _fileSystemService.ReadFileAsync(filePath);

        return new EditorTab
        {
            FilePath = filePath,
            DisplayName = Path.GetFileName(filePath),
            Content = content,
            Language = DetectLanguage(filePath),
            IsModified = false
        };
    }

    public async Task SaveFileAsync(EditorTab tab)
    {
        await _fileSystemService.WriteFileAsync(tab.FilePath, tab.Content);
        tab.IsModified = false;
    }

    public string DetectLanguage(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".cs" => "csharp",
            ".xaml" or ".xml" or ".csproj" or ".sln" or ".slnx" => "xml",
            ".json" => "json",
            ".md" => "markdown",
            ".html" or ".htm" => "html",
            ".css" => "css",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".py" => "python",
            ".sh" or ".bash" => "shell",
            ".yaml" or ".yml" => "yaml",
            ".sql" => "sql",
            ".txt" or ".log" => "plaintext",
            _ => "plaintext"
        };
    }
}
