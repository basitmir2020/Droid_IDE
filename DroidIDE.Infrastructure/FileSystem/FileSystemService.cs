using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.Infrastructure.FileSystem;

/// <summary>
/// Concrete implementation of IFileSystemService using System.IO.
/// </summary>
public class FileSystemService : IFileSystemService
{
    public async Task<string> ReadFileAsync(string path)
    {
        return await File.ReadAllTextAsync(path);
    }

    public async Task WriteFileAsync(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(path, content);
    }

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

    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public async Task CreateFileAsync(string path, string content = "")
    {
        await WriteFileAsync(path, content);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public Task DeleteAsync(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
        else if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);

        return Task.CompletedTask;
    }

    public Task RenameAsync(string oldPath, string newPath)
    {
        if (File.Exists(oldPath))
            File.Move(oldPath, newPath);
        else if (Directory.Exists(oldPath))
            Directory.Move(oldPath, newPath);

        return Task.CompletedTask;
    }

    public string GetExtension(string path) => Path.GetExtension(path).TrimStart('.');
}
