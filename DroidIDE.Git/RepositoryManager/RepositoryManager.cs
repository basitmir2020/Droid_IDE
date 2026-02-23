using DroidIDE.Core.Enums;
using DroidIDE.Core.Interfaces;

namespace DroidIDE.Git.RepositoryManager;

/// <summary>
/// Manages Git repository lifecycle — initialization, cloning, and discovery.
/// Provides a high-level wrapper over IGitService for repository-level operations.
/// </summary>
public class RepositoryManager
{
    private readonly IGitService _gitService;
    private string? _currentRepoPath;

    public RepositoryManager(IGitService gitService)
    {
        _gitService = gitService;
    }

    /// <summary>Path of the currently tracked repository.</summary>
    public string? CurrentRepoPath => _currentRepoPath;

    /// <summary>Whether a repository is currently loaded.</summary>
    public bool HasRepository => _currentRepoPath is not null;

    /// <summary>Fired when a repository is opened or changed.</summary>
    public event Action<string>? RepositoryOpened;

    /// <summary>Fired when repository status is refreshed.</summary>
    public event Action<Dictionary<string, GitFileStatus>>? StatusRefreshed;

    /// <summary>
    /// Open an existing Git repository by path.
    /// </summary>
    public void OpenRepository(string path)
    {
        if (!_gitService.IsRepository(path))
            throw new InvalidOperationException($"'{path}' is not a Git repository.");

        _currentRepoPath = path;
        RepositoryOpened?.Invoke(path);
    }

    /// <summary>
    /// Try to discover and open a Git repository from a directory.
    /// Walks up the directory tree looking for a .git folder.
    /// </summary>
    public bool TryDiscoverRepository(string directoryPath)
    {
        var current = directoryPath;
        while (!string.IsNullOrEmpty(current))
        {
            if (_gitService.IsRepository(current))
            {
                _currentRepoPath = current;
                RepositoryOpened?.Invoke(current);
                return true;
            }
            current = Path.GetDirectoryName(current);
        }
        return false;
    }

    /// <summary>
    /// Initialize a new repository at the given path.
    /// </summary>
    public async Task InitializeAsync(string path)
    {
        await _gitService.InitAsync(path);
        _currentRepoPath = path;
        RepositoryOpened?.Invoke(path);
    }

    /// <summary>
    /// Clone a remote repository to a target directory.
    /// </summary>
    public async Task CloneAsync(string url, string targetPath, IProgress<string>? progress = null)
    {
        await _gitService.CloneAsync(url, targetPath, progress);
        _currentRepoPath = targetPath;
        RepositoryOpened?.Invoke(targetPath);
    }

    /// <summary>
    /// Quick commit: stage all + commit with message.
    /// </summary>
    public async Task QuickCommitAsync(string message, string authorName, string authorEmail)
    {
        if (_currentRepoPath is null)
            throw new InvalidOperationException("No repository is currently open.");

        await _gitService.StageAllAsync(_currentRepoPath);
        await _gitService.CommitAsync(_currentRepoPath, message, authorName, authorEmail);
    }

    /// <summary>
    /// Refresh the file status of the current repository.
    /// </summary>
    public async Task<Dictionary<string, GitFileStatus>> RefreshStatusAsync()
    {
        if (_currentRepoPath is null)
            return new Dictionary<string, GitFileStatus>();

        var status = await _gitService.GetStatusAsync(_currentRepoPath);
        StatusRefreshed?.Invoke(status);
        return status;
    }

    /// <summary>
    /// Sync with remote: pull then push.
    /// </summary>
    public async Task SyncAsync(string remoteName = "origin")
    {
        if (_currentRepoPath is null)
            throw new InvalidOperationException("No repository is currently open.");

        await _gitService.PullAsync(_currentRepoPath, remoteName);

        var branch = await _gitService.GetCurrentBranchAsync(_currentRepoPath);
        await _gitService.PushAsync(_currentRepoPath, remoteName, branch);
    }
}
