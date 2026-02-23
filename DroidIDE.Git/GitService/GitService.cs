using DroidIDE.Core.Enums;
using DroidIDE.Core.Interfaces;
using LibGit2Sharp;

namespace DroidIDE.Git.GitService;

/// <summary>
/// Implements <see cref="IGitService"/> using LibGit2Sharp for all Git operations.
/// Provides repository initialization, cloning, staging, committing, pushing, pulling,
/// status queries, and branch management — all running on background threads via <see cref="Task.Run"/>.
/// </summary>
public class GitService : IGitService
{
    /// <inheritdoc/>
    public Task InitAsync(string path)
    {
        return Task.Run(() => Repository.Init(path));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Reports clone progress via the optional <paramref name="progress"/> callback
    /// using the LibGit2Sharp transfer progress handler.
    /// </remarks>
    public Task CloneAsync(string url, string targetPath, IProgress<string>? progress = null)
    {
        return Task.Run(() =>
        {
            var options = new CloneOptions();
            if (progress is not null)
            {
                options.FetchOptions.OnTransferProgress = transferProgress =>
                {
                    progress.Report(
                        $"Receiving objects: {transferProgress.ReceivedObjects}/{transferProgress.TotalObjects}");
                    return true;
                };
            }

            Repository.Clone(url, targetPath, options);
        });
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Converts absolute file paths to repository-relative paths before staging.
    /// </remarks>
    public Task StageAsync(string repoPath, IEnumerable<string> filePaths)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            foreach (var filePath in filePaths)
            {
                // Convert to relative path within the repo
                var relativePath = Path.GetRelativePath(repoPath, filePath);
                Commands.Stage(repo, relativePath);
            }
        });
    }

    /// <inheritdoc/>
    public Task StageAllAsync(string repoPath)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            Commands.Stage(repo, "*");
        });
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Uses the same <see cref="Signature"/> for both author and committer fields.
    /// </remarks>
    public Task CommitAsync(string repoPath, string message, string authorName, string authorEmail)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            var author = new Signature(authorName, authorEmail, DateTimeOffset.Now);
            repo.Commit(message, author, author);
        });
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the specified remote or branch is not found in the repository.
    /// </exception>
    public Task PushAsync(string repoPath, string remoteName = "origin", string branchName = "main")
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            var remote = repo.Network.Remotes[remoteName];
            if (remote is null)
                throw new InvalidOperationException($"Remote '{remoteName}' not found.");

            var branch = repo.Branches[branchName];
            if (branch is null)
                throw new InvalidOperationException($"Branch '{branchName}' not found.");

            repo.Network.Push(branch);
        });
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Builds the commit signature from the repository's git config (user.name and user.email).
    /// </remarks>
    public Task PullAsync(string repoPath, string remoteName = "origin")
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            var signature = repo.Config.BuildSignature(DateTimeOffset.Now);

            var options = new PullOptions
            {
                FetchOptions = new FetchOptions()
            };

            Commands.Pull(repo, signature, options);
        });
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Only returns files with a non-unmodified status. Unmodified files are excluded
    /// from the result dictionary to keep the output compact.
    /// </remarks>
    public Task<Dictionary<string, GitFileStatus>> GetStatusAsync(string repoPath)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            var result = new Dictionary<string, GitFileStatus>();

            foreach (var entry in repo.RetrieveStatus(new StatusOptions()))
            {
                var status = MapStatus(entry.State);
                if (status != GitFileStatus.Unmodified)
                {
                    result[entry.FilePath] = status;
                }
            }

            return result;
        });
    }

    /// <inheritdoc/>
    /// <remarks>Returns only local branches; remote tracking branches are excluded.</remarks>
    public Task<List<string>> GetBranchesAsync(string repoPath)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            return repo.Branches
                .Where(b => !b.IsRemote)
                .Select(b => b.FriendlyName)
                .ToList();
        });
    }

    /// <inheritdoc/>
    public Task<string> GetCurrentBranchAsync(string repoPath)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            return repo.Head.FriendlyName;
        });
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when the specified branch is not found.</exception>
    public Task CheckoutAsync(string repoPath, string branchName)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            var branch = repo.Branches[branchName]
                ?? throw new InvalidOperationException($"Branch '{branchName}' not found.");
            Commands.Checkout(repo, branch);
        });
    }

    /// <inheritdoc/>
    public Task CreateBranchAsync(string repoPath, string branchName)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(repoPath);
            repo.CreateBranch(branchName);
        });
    }

    /// <inheritdoc/>
    public bool IsRepository(string path)
    {
        return Repository.IsValid(path);
    }

    /// <summary>
    /// Maps LibGit2Sharp's <see cref="FileStatus"/> flags to the DroidIDE Core
    /// <see cref="GitFileStatus"/> enum for UI display.
    /// </summary>
    /// <param name="status">The LibGit2Sharp file status flags.</param>
    /// <returns>The corresponding <see cref="GitFileStatus"/> value.</returns>
    private static GitFileStatus MapStatus(FileStatus status)
    {
        if (status.HasFlag(FileStatus.Conflicted))
            return GitFileStatus.Conflicted;
        if (status.HasFlag(FileStatus.NewInIndex) || status.HasFlag(FileStatus.NewInWorkdir))
            return GitFileStatus.Added;
        if (status.HasFlag(FileStatus.DeletedFromIndex) || status.HasFlag(FileStatus.DeletedFromWorkdir))
            return GitFileStatus.Deleted;
        if (status.HasFlag(FileStatus.RenamedInIndex) || status.HasFlag(FileStatus.RenamedInWorkdir))
            return GitFileStatus.Renamed;
        if (status.HasFlag(FileStatus.ModifiedInIndex) || status.HasFlag(FileStatus.ModifiedInWorkdir))
            return GitFileStatus.Modified;
        if (status.HasFlag(FileStatus.Ignored))
            return GitFileStatus.Ignored;
        if (status.HasFlag(FileStatus.NewInWorkdir))
            return GitFileStatus.Untracked;

        return GitFileStatus.Unmodified;
    }
}
