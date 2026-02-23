using DroidIDE.Core.Enums;

namespace DroidIDE.Core.Interfaces;

/// <summary>
/// Abstracts Git version control operations for source control functionality within the IDE.
/// Implemented by <c>GitService</c> in the Git layer using LibGit2Sharp.
/// </summary>
public interface IGitService
{
    /// <summary>Initializes a new Git repository at the specified path.</summary>
    /// <param name="path">The directory path where the repository will be created.</param>
    Task InitAsync(string path);

    /// <summary>Clones a remote Git repository to a local directory.</summary>
    /// <param name="url">The remote repository URL (HTTPS or SSH).</param>
    /// <param name="targetPath">The local directory path to clone into.</param>
    /// <param name="progress">Optional progress reporter for clone status updates.</param>
    Task CloneAsync(string url, string targetPath, IProgress<string>? progress = null);

    /// <summary>Stages specific files for the next commit.</summary>
    /// <param name="repoPath">The root path of the Git repository.</param>
    /// <param name="filePaths">The file paths (relative to repo root) to stage.</param>
    Task StageAsync(string repoPath, IEnumerable<string> filePaths);

    /// <summary>Stages all changed files in the working directory for the next commit.</summary>
    /// <param name="repoPath">The root path of the Git repository.</param>
    Task StageAllAsync(string repoPath);

    /// <summary>Creates a new commit from the currently staged changes.</summary>
    /// <param name="repoPath">The root path of the Git repository.</param>
    /// <param name="message">The commit message.</param>
    /// <param name="authorName">The author's display name.</param>
    /// <param name="authorEmail">The author's email address.</param>
    Task CommitAsync(string repoPath, string message, string authorName, string authorEmail);

    /// <summary>Pushes local commits to a remote repository.</summary>
    /// <param name="repoPath">The root path of the Git repository.</param>
    /// <param name="remoteName">The remote name (defaults to "origin").</param>
    /// <param name="branchName">The branch name to push (defaults to "main").</param>
    Task PushAsync(string repoPath, string remoteName = "origin", string branchName = "main");

    /// <summary>Pulls changes from a remote repository and merges them into the current branch.</summary>
    /// <param name="repoPath">The root path of the Git repository.</param>
    /// <param name="remoteName">The remote name (defaults to "origin").</param>
    Task PullAsync(string repoPath, string remoteName = "origin");

    /// <summary>Retrieves the status of all files in the working directory.</summary>
    /// <param name="repoPath">The root path of the Git repository.</param>
    /// <returns>A dictionary mapping relative file paths to their <see cref="GitFileStatus"/> values.</returns>
    Task<Dictionary<string, GitFileStatus>> GetStatusAsync(string repoPath);

    /// <summary>Lists all branch names in the repository (local and remote-tracking).</summary>
    /// <param name="repoPath">The root path of the Git repository.</param>
    /// <returns>A list of branch name strings.</returns>
    Task<List<string>> GetBranchesAsync(string repoPath);

    /// <summary>Gets the name of the currently checked-out branch.</summary>
    /// <param name="repoPath">The root path of the Git repository.</param>
    /// <returns>The current branch name (e.g., "main", "feature/login").</returns>
    Task<string> GetCurrentBranchAsync(string repoPath);

    /// <summary>Checks out an existing branch, switching the working directory to that branch's state.</summary>
    /// <param name="repoPath">The root path of the Git repository.</param>
    /// <param name="branchName">The name of the branch to check out.</param>
    Task CheckoutAsync(string repoPath, string branchName);

    /// <summary>Creates a new branch at the current HEAD position.</summary>
    /// <param name="repoPath">The root path of the Git repository.</param>
    /// <param name="branchName">The name for the new branch.</param>
    Task CreateBranchAsync(string repoPath, string branchName);

    /// <summary>Determines whether the specified directory is a Git repository.</summary>
    /// <param name="path">The directory path to check.</param>
    /// <returns><c>true</c> if a .git directory or repository database exists; otherwise, <c>false</c>.</returns>
    bool IsRepository(string path);
}
