using DroidIDE.Core.Interfaces;

namespace DroidIDE.Git.BranchManager;

/// <summary>
/// High-level branch management operations built on top of <see cref="IGitService"/>.
/// Provides convenience methods for common branch workflows such as creating, switching,
/// and querying branches, with a <see cref="BranchChanged"/> event for UI binding.
/// </summary>
public class BranchManager
{
    private readonly IGitService _gitService;

    /// <summary>
    /// Initializes a new instance of <see cref="BranchManager"/>.
    /// </summary>
    /// <param name="gitService">The Git service used for underlying branch operations.</param>
    public BranchManager(IGitService gitService)
    {
        _gitService = gitService;
    }

    /// <summary>Raised when the active branch changes after a checkout or branch creation.</summary>
    public event Action<string>? BranchChanged;

    /// <summary>
    /// Gets the current (HEAD) branch name for a repository.
    /// </summary>
    /// <param name="repoPath">The absolute path to the Git repository root.</param>
    /// <returns>The friendly name of the current branch (e.g., "main", "feature/xyz").</returns>
    public async Task<string> GetCurrentBranchAsync(string repoPath)
    {
        return await _gitService.GetCurrentBranchAsync(repoPath);
    }

    /// <summary>
    /// Gets a list of all local branch names in the repository.
    /// </summary>
    /// <param name="repoPath">The absolute path to the Git repository root.</param>
    /// <returns>A list of local branch friendly names.</returns>
    public async Task<List<string>> GetAllBranchesAsync(string repoPath)
    {
        return await _gitService.GetBranchesAsync(repoPath);
    }

    /// <summary>
    /// Creates a new branch and immediately checks it out, firing <see cref="BranchChanged"/>.
    /// </summary>
    /// <param name="repoPath">The absolute path to the Git repository root.</param>
    /// <param name="branchName">The name of the new branch to create.</param>
    public async Task CreateAndCheckoutAsync(string repoPath, string branchName)
    {
        await _gitService.CreateBranchAsync(repoPath, branchName);
        await _gitService.CheckoutAsync(repoPath, branchName);
        BranchChanged?.Invoke(branchName);
    }

    /// <summary>
    /// Switches to an existing branch, firing <see cref="BranchChanged"/>.
    /// </summary>
    /// <param name="repoPath">The absolute path to the Git repository root.</param>
    /// <param name="branchName">The name of the branch to switch to.</param>
    public async Task SwitchBranchAsync(string repoPath, string branchName)
    {
        await _gitService.CheckoutAsync(repoPath, branchName);
        BranchChanged?.Invoke(branchName);
    }

    /// <summary>
    /// Checks whether a branch with the given name exists in the repository.
    /// </summary>
    /// <param name="repoPath">The absolute path to the Git repository root.</param>
    /// <param name="branchName">The branch name to check (case-insensitive).</param>
    /// <returns><c>true</c> if the branch exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> BranchExistsAsync(string repoPath, string branchName)
    {
        var branches = await _gitService.GetBranchesAsync(repoPath);
        return branches.Contains(branchName, StringComparer.OrdinalIgnoreCase);
    }
}
