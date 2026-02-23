using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Abstracts file system operations for reading, writing, and browsing files.
/// </summary>
public interface IFileSystemService
{
    /// <summary>Read the full text content of a file.</summary>
    Task<string> ReadFileAsync(string path);

    /// <summary>Write text content to a file, creating it if necessary.</summary>
    Task WriteFileAsync(string path, string content);

    /// <summary>List all files and directories within a directory.</summary>
    Task<List<FileItem>> ListDirectoryAsync(string path);

    /// <summary>Build a recursive file tree from a root directory.</summary>
    Task<FileItem> GetFileTreeAsync(string rootPath);

    /// <summary>Check if a file exists.</summary>
    bool FileExists(string path);

    /// <summary>Check if a directory exists.</summary>
    bool DirectoryExists(string path);

    /// <summary>Create a new file with optional content.</summary>
    Task CreateFileAsync(string path, string content = "");

    /// <summary>Create a new directory.</summary>
    void CreateDirectory(string path);

    /// <summary>Delete a file or directory.</summary>
    Task DeleteAsync(string path);

    /// <summary>Rename/move a file or directory.</summary>
    Task RenameAsync(string oldPath, string newPath);

    /// <summary>Get the file extension without the dot.</summary>
    string GetExtension(string path);
}
