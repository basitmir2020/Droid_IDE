namespace DroidIDE.Core.Models;

/// <summary>
/// Represents a file or folder node in the project explorer tree.
/// Used by <see cref="DroidIDE.Core.Interfaces.IFileSystemService"/> to build hierarchical directory structures.
/// </summary>
public class FileItem
{
    /// <summary>Gets or sets the file or directory name (e.g., "Program.cs").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the absolute path to the file or directory.</summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this item is a directory.</summary>
    public bool IsDirectory { get; set; }

    /// <summary>Gets or sets the file extension including the leading dot (e.g., ".cs"). Empty for directories.</summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>Gets or sets the file size in bytes. Zero for directories.</summary>
    public long SizeBytes { get; set; }

    /// <summary>Gets or sets the last modification timestamp of the file or directory.</summary>
    public DateTime LastModified { get; set; }

    /// <summary>Gets or sets the child items when this item is a directory.</summary>
    public List<FileItem> Children { get; set; } = [];

    /// <summary>Gets or sets a value indicating whether this directory node is expanded in the explorer tree.</summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Gets the icon identifier based on the file extension.
    /// Directories return "folder"; files return a language-specific icon name.
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
