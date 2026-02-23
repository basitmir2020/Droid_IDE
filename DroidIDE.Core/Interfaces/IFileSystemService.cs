using DroidIDE.Core.Models;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Abstracts file system operations for reading, writing, and browsing files and directories.
/// Implemented by <c>FileSystemService</c> in the Infrastructure layer.
/// </summary>
public interface IFileSystemService
{
    /// <summary>Reads the full text content of a file asynchronously.</summary>
    /// <param name="path">The absolute path to the file to read.</param>
    /// <returns>The file content as a string.</returns>
    Task<string> ReadFileAsync(string path);

    /// <summary>Writes text content to a file, creating the file and parent directories if necessary.</summary>
    /// <param name="path">The absolute path to the file to write.</param>
    /// <param name="content">The text content to write.</param>
    Task WriteFileAsync(string path, string content);

    /// <summary>Lists all files and directories within a given directory (non-recursive).</summary>
    /// <param name="path">The absolute path to the directory to list.</param>
    /// <returns>A list of <see cref="FileItem"/> entries sorted alphabetically with directories first.</returns>
    Task<List<FileItem>> ListDirectoryAsync(string path);

    /// <summary>Builds a recursive file tree starting from a root directory.</summary>
    /// <param name="rootPath">The absolute path to the root directory.</param>
    /// <returns>A <see cref="FileItem"/> representing the root with nested <see cref="FileItem.Children"/>.</returns>
    Task<FileItem> GetFileTreeAsync(string rootPath);

    /// <summary>Checks whether a file exists at the specified path.</summary>
    /// <param name="path">The absolute path to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    bool FileExists(string path);

    /// <summary>Checks whether a directory exists at the specified path.</summary>
    /// <param name="path">The absolute path to check.</param>
    /// <returns><c>true</c> if the directory exists; otherwise, <c>false</c>.</returns>
    bool DirectoryExists(string path);

    /// <summary>Creates a new file with optional initial content. Creates parent directories if needed.</summary>
    /// <param name="path">The absolute path for the new file.</param>
    /// <param name="content">Optional initial content to write to the file.</param>
    Task CreateFileAsync(string path, string content = "");

    /// <summary>Creates a new directory at the specified path, including any missing parent directories.</summary>
    /// <param name="path">The absolute path of the directory to create.</param>
    void CreateDirectory(string path);

    /// <summary>Deletes a file or directory at the specified path.</summary>
    /// <param name="path">The absolute path of the file or directory to delete.</param>
    Task DeleteAsync(string path);

    /// <summary>Renames or moves a file or directory from one path to another.</summary>
    /// <param name="oldPath">The current absolute path.</param>
    /// <param name="newPath">The new absolute path.</param>
    Task RenameAsync(string oldPath, string newPath);

    /// <summary>Gets the file extension from a path, without the leading dot.</summary>
    /// <param name="path">The file path to extract the extension from.</param>
    /// <returns>The file extension without the dot (e.g., "cs"), or an empty string for extensionless files.</returns>
    string GetExtension(string path);
}
