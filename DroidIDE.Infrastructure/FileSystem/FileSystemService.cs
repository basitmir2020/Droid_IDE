using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.Infrastructure.FileSystem;

/// <summary>
/// Concrete implementation of <see cref="IFileSystemService"/> using <c>System.IO</c>.
/// Provides file and directory operations for the IDE's project explorer, editor, and file management.
/// </summary>
public class FileSystemService : IFileSystemService
{
    /// <inheritdoc />
    public async Task<string> ReadFileAsync(string path)
    {
        return await File.ReadAllTextAsync(path);
    }

    /// <inheritdoc />
    /// <remarks>Automatically creates parent directories if they do not exist.</remarks>
    public async Task WriteFileAsync(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(path, content);
    }

    /// <inheritdoc />
    /// <remarks>Returns directories first, then files, both sorted alphabetically by name.</remarks>
    public Task<List<FileItem>> ListDirectoryAsync(string path)
    {
        var items = new List<FileItem>();

        if (!Directory.Exists(path))
            return Task.FromResult(items);

        // Directories first, then files, both alphabetically sorted
        foreach (var dir in Directory.GetDirectories(path).OrderBy(d => Path.GetFileName(d)))
        {
            items.Add(new FileItem
            {
                Name = Path.GetFileName(dir),
                FullPath = dir,
                IsDirectory = true,
                LastModified = Directory.GetLastWriteTimeUtc(dir)
            });
        }

        foreach (var file in Directory.GetFiles(path).OrderBy(f => Path.GetFileName(f)))
        {
            var info = new FileInfo(file);
            items.Add(new FileItem
            {
                Name = info.Name,
                FullPath = info.FullName,
                IsDirectory = false,
                Extension = info.Extension,
                SizeBytes = info.Length,
                LastModified = info.LastWriteTimeUtc
            });
        }

        return Task.FromResult(items);
    }

    /// <inheritdoc />
    /// <remarks>Recursively populates the tree up to a depth of 5 levels to avoid scanning excessively deep trees.</remarks>
    public async Task<FileItem> GetFileTreeAsync(string rootPath)
    {
        var rootName = Path.GetFileName(rootPath);
        if (string.IsNullOrEmpty(rootName))
            rootName = rootPath;

        var root = new FileItem
        {
            Name = rootName,
            FullPath = rootPath,
            IsDirectory = true,
            IsExpanded = true
        };

        await PopulateChildrenAsync(root, maxDepth: 5);
        return root;
    }

    /// <summary>
    /// Recursively populates the <see cref="FileItem.Children"/> collection for a directory node.
    /// </summary>
    /// <param name="parent">The parent directory node to populate.</param>
    /// <param name="maxDepth">The maximum recursion depth to prevent scanning excessively deep trees.</param>
    /// <param name="currentDepth">The current recursion depth (starts at 0).</param>
    private async Task PopulateChildrenAsync(FileItem parent, int maxDepth, int currentDepth = 0)
    {
        if (currentDepth >= maxDepth || !Directory.Exists(parent.FullPath))
            return;

        var children = await ListDirectoryAsync(parent.FullPath);
        parent.Children = children;

        foreach (var child in children.Where(c => c.IsDirectory))
        {
            await PopulateChildrenAsync(child, maxDepth, currentDepth + 1);
        }
    }

    /// <inheritdoc />
    public bool FileExists(string path) => File.Exists(path);

    /// <inheritdoc />
    public bool DirectoryExists(string path) => Directory.Exists(path);

    /// <inheritdoc />
    /// <remarks>Delegates to <see cref="WriteFileAsync"/> to ensure parent directory creation.</remarks>
    public async Task CreateFileAsync(string path, string content = "")
    {
        await WriteFileAsync(path, content);
    }

    /// <inheritdoc />
    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    /// <inheritdoc />
    /// <remarks>Handles both files and directories. Directories are deleted recursively.</remarks>
    public Task DeleteAsync(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
        else if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <remarks>Uses <c>File.Move</c> for files and <c>Directory.Move</c> for directories.</remarks>
    public Task RenameAsync(string oldPath, string newPath)
    {
        if (File.Exists(oldPath))
            File.Move(oldPath, newPath);
        else if (Directory.Exists(oldPath))
            Directory.Move(oldPath, newPath);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public string GetExtension(string path) => Path.GetExtension(path).TrimStart('.');
}
