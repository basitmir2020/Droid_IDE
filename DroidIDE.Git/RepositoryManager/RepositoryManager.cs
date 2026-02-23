using DroidIDE.Core.Enums;
using DroidIDE.Core.Interfaces;

namespace DroidIDE.Git.RepositoryManager;

/// <summary>
/// Manages Git repository lifecycle — initialization, cloning, discovery, and high-level
/// operations like quick commit and sync. Provides a stateful wrapper over <see cref="IGitService"/>
/// that tracks the currently open repository and fires events for UI binding.
/// </summary>
public class RepositoryManager
{
    private readonly IGitService _gitService;
    private string? _currentRepoPath;

    /// <summary>
    /// Initializes a new instance of <see cref="RepositoryManager"/>.
    /// </summary>
    /// <param name="gitService">The Git service used for underlying repository operations.</param>
    public RepositoryManager(IGitService gitService)
    {
        _gitService = gitService;
    }

    /// <summary>Gets the absolute path of the currently tracked repository, or <c>null</c> if none is open.</summary>
    public string? CurrentRepoPath => _currentRepoPath;

    /// <summary>Gets whether a repository is currently loaded and tracked.</summary>
    public bool HasRepository => _currentRepoPath is not null;

    /// <summary>Raised when a repository is opened, initialized, or cloned, providing the repository path.</summary>
    public event Action<string>? RepositoryOpened;

    /// <summary>Raised when the repository file status is refreshed, providing the file-to-status mapping.</summary>
    public event Action<Dictionary<string, GitFileStatus>>? StatusRefreshed;

    /// <summary>
    /// Opens an existing Git repository at the specified path.
    /// </summary>
    /// <param name="path">The absolute path to the Git repository root.</param>
    /// <exception cref="InvalidOperationException">Thrown if the path is not a valid Git repository.</exception>
    public void OpenRepository(string path)
    {
        if (!_gitService.IsRepository(path))
            throw new InvalidOperationException($"'{path}' is not a Git repository.");

        _currentRepoPath = path;
        RepositoryOpened?.Invoke(path);
    }

    /// <summary>
    /// Attempts to discover a Git repository by walking up the directory tree from the given path.
    /// </summary>
    /// <param name="directoryPath">The starting directory path to search from.</param>
    /// <returns><c>true</c> if a repository was found and opened; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Walks up parent directories looking for a valid Git repository (containing a .git folder).
    /// Stops at the filesystem root if no repository is found.
    /// </remarks>
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
    /// Initializes a new Git repository at the given path and sets it as the current repository.
    /// </summary>
    /// <param name="path">The absolute directory path where the repository should be initialized.</param>
    public async Task InitializeAsync(string path)
    {
        await _gitService.InitAsync(path);
        _currentRepoPath = path;
        RepositoryOpened?.Invoke(path);
    }

    /// <summary>
    /// Clones a remote Git repository to a target directory and sets it as the current repository.
    /// </summary>
    /// <param name="url">The remote repository URL to clone from.</param>
    /// <param name="targetPath">The local directory path to clone into.</param>
    /// <param name="progress">Optional progress reporter for clone transfer updates.</param>
    public async Task CloneAsync(string url, string targetPath, IProgress<string>? progress = null)
    {
        await _gitService.CloneAsync(url, targetPath, progress);
        _currentRepoPath = targetPath;
        RepositoryOpened?.Invoke(targetPath);
    }

    /// <summary>
    /// Performs a quick commit: stages all changes and commits with the specified message.
    /// </summary>
    /// <param name="message">The commit message.</param>
    /// <param name="authorName">The author's name for the commit signature.</param>
    /// <param name="authorEmail">The author's email for the commit signature.</param>
    /// <exception cref="InvalidOperationException">Thrown if no repository is currently open.</exception>
    public async Task QuickCommitAsync(string message, string authorName, string authorEmail)
    {
        if (_currentRepoPath is null)
            throw new InvalidOperationException("No repository is currently open.");

        await _gitService.StageAllAsync(_currentRepoPath);
        await _gitService.CommitAsync(_currentRepoPath, message, authorName, authorEmail);
    }

    /// <summary>
    /// Refreshes the file status of the current repository and fires <see cref="StatusRefreshed"/>.
    /// </summary>
    /// <returns>
    /// A dictionary mapping relative file paths to their <see cref="GitFileStatus"/>.
    /// Returns an empty dictionary if no repository is open.
    /// </returns>
    public async Task<Dictionary<string, GitFileStatus>> RefreshStatusAsync()
    {
        if (_currentRepoPath is null)
            return new Dictionary<string, GitFileStatus>();

        var status = await _gitService.GetStatusAsync(_currentRepoPath);
        StatusRefreshed?.Invoke(status);
        return status;
    }

    /// <summary>
    /// Synchronizes with a remote by pulling then pushing the current branch.
    /// </summary>
    /// <param name="remoteName">The remote name to sync with (defaults to "origin").</param>
    /// <exception cref="InvalidOperationException">Thrown if no repository is currently open.</exception>
    public async Task SyncAsync(string remoteName = "origin")
    {
        if (_currentRepoPath is null)
            throw new InvalidOperationException("No repository is currently open.");

        await _gitService.PullAsync(_currentRepoPath, remoteName);

        var branch = await _gitService.GetCurrentBranchAsync(_currentRepoPath);
        await _gitService.PushAsync(_currentRepoPath, remoteName, branch);
    }
}
