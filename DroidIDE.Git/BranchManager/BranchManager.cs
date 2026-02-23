using DroidIDE.Core.Interfaces;

namespace DroidIDE.Git.BranchManager;

/// <summary>
/// High-level branch management operations built on top of IGitService.
/// Provides convenience methods for common branch workflows.
/// </summary>
public class BranchManager
{
    private readonly IGitService _gitService;

    public BranchManager(IGitService gitService)
    {
        _gitService = gitService;
    }

    /// <summary>Fired when the current branch changes.</summary>
    public event Action<string>? BranchChanged;

    /// <summary>
    /// Get the current branch name for a repository.
    /// </summary>
    public async Task<string> GetCurrentBranchAsync(string repoPath)
    {
        return await _gitService.GetCurrentBranchAsync(repoPath);
    }

    /// <summary>
    /// Get all local branches.
    /// </summary>
    public async Task<List<string>> GetAllBranchesAsync(string repoPath)
    {
        return await _gitService.GetBranchesAsync(repoPath);
    }

    /// <summary>
    /// Create and checkout a new branch.
    /// </summary>
    public async Task CreateAndCheckoutAsync(string repoPath, string branchName)
    {
        await _gitService.CreateBranchAsync(repoPath, branchName);
        await _gitService.CheckoutAsync(repoPath, branchName);
        BranchChanged?.Invoke(branchName);
    }

    /// <summary>
    /// Switch to an existing branch.
    /// </summary>
    public async Task SwitchBranchAsync(string repoPath, string branchName)
    {
        await _gitService.CheckoutAsync(repoPath, branchName);
        BranchChanged?.Invoke(branchName);
    }

    /// <summary>
    /// Check if a branch exists.
    /// </summary>
    public async Task<bool> BranchExistsAsync(string repoPath, string branchName)
    {
        var branches = await _gitService.GetBranchesAsync(repoPath);
        return branches.Contains(branchName, StringComparer.OrdinalIgnoreCase);
    }
}
