using DroidIDE.Core.Enums;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Abstracts Git operations for source control functionality.
/// </summary>
public interface IGitService
{
    /// <summary>Initialize a new Git repository.</summary>
    Task InitAsync(string path);

    /// <summary>Clone a remote repository.</summary>
    Task CloneAsync(string url, string targetPath, IProgress<string>? progress = null);

    /// <summary>Stage files for commit.</summary>
    Task StageAsync(string repoPath, IEnumerable<string> filePaths);

    /// <summary>Stage all changed files.</summary>
    Task StageAllAsync(string repoPath);

    /// <summary>Commit staged changes.</summary>
    Task CommitAsync(string repoPath, string message, string authorName, string authorEmail);

    /// <summary>Push commits to the remote.</summary>
    Task PushAsync(string repoPath, string remoteName = "origin", string branchName = "main");

    /// <summary>Pull changes from the remote.</summary>
    Task PullAsync(string repoPath, string remoteName = "origin");

    /// <summary>Get the status of all files in the working directory.</summary>
    Task<Dictionary<string, GitFileStatus>> GetStatusAsync(string repoPath);

    /// <summary>Get all branches in the repository.</summary>
    Task<List<string>> GetBranchesAsync(string repoPath);

    /// <summary>Get the current branch name.</summary>
    Task<string> GetCurrentBranchAsync(string repoPath);

    /// <summary>Checkout a branch.</summary>
    Task CheckoutAsync(string repoPath, string branchName);

    /// <summary>Create a new branch.</summary>
    Task CreateBranchAsync(string repoPath, string branchName);

    /// <summary>Check if a directory is a Git repository.</summary>
    bool IsRepository(string path);
}
