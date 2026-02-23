namespace DroidIDE.Core.Models;

/// <summary>
/// Represents a file or folder in the project explorer tree.
/// </summary>
public class FileItem
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public string Extension { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }
    public List<FileItem> Children { get; set; } = [];
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Returns the icon name based on file extension.
    /// </summary>
    public string IconName => IsDirectory ? "folder" : Extension.ToLowerInvariant() switch
    {
        ".cs" => "csharp",
        ".xaml" => "xml",
        ".xml" => "xml",
        ".json" => "json",
        ".csproj" => "project",
        ".sln" or ".slnx" => "solution",
        ".md" => "markdown",
        ".txt" => "text",
        ".css" => "css",
        ".html" or ".htm" => "html",
        ".js" => "javascript",
        ".ts" => "typescript",
        _ => "file"
    };
}
