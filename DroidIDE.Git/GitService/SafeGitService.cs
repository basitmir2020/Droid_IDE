using DroidIDE.Core.Enums;
using DroidIDE.Core.Interfaces;

namespace DroidIDE.Git.GitService;

/// <summary>
/// A resilient Git service implementation that wraps the primary LibGit2Sharp service
/// and falls back to a CLI-based service if the native libraries fail to load.
/// </summary>
public class SafeGitService : IGitService
{
    private IGitService? _primaryInstance;
    private readonly IGitService _fallback;
    private bool _useFallback;

    public SafeGitService(CliGitService fallback, bool useFallbackInitially = false)
    {
        _fallback = fallback;
        _useFallback = useFallbackInitially;
        
        if (_useFallback)
        {
            System.Diagnostics.Debug.WriteLine("[SafeGitService] Initialized with fallback active.");
        }
    }

    private IGitService GetPrimary()
    {
        return _primaryInstance ??= new GitService();
    }

    public async Task InitAsync(string path) => await ExecuteSafeAsync(s => s.InitAsync(path));

    public async Task CloneAsync(string url, string targetPath, IProgress<string>? progress = null) 
        => await ExecuteSafeAsync(s => s.CloneAsync(url, targetPath, progress));

    public async Task StageAsync(string repoPath, IEnumerable<string> filePaths) 
        => await ExecuteSafeAsync(s => s.StageAsync(repoPath, filePaths));

    public async Task StageAllAsync(string repoPath) 
        => await ExecuteSafeAsync(s => s.StageAllAsync(repoPath));

    public async Task CommitAsync(string repoPath, string message, string authorName, string authorEmail) 
        => await ExecuteSafeAsync(s => s.CommitAsync(repoPath, message, authorName, authorEmail));

    public async Task PushAsync(string repoPath, string remoteName = "origin", string branchName = "main") 
        => await ExecuteSafeAsync(s => s.PushAsync(repoPath, remoteName, branchName));

    public async Task PullAsync(string repoPath, string remoteName = "origin") 
        => await ExecuteSafeAsync(s => s.PullAsync(repoPath, remoteName));

    public async Task<Dictionary<string, GitFileStatus>> GetStatusAsync(string repoPath) 
        => await ExecuteSafeAsync(s => s.GetStatusAsync(repoPath));

    public async Task<List<string>> GetBranchesAsync(string repoPath) 
        => await ExecuteSafeAsync(s => s.GetBranchesAsync(repoPath));

    public async Task<string> GetCurrentBranchAsync(string repoPath) 
        => await ExecuteSafeAsync(s => s.GetCurrentBranchAsync(repoPath));

    public async Task CheckoutAsync(string repoPath, string branchName) 
        => await ExecuteSafeAsync(s => s.CheckoutAsync(repoPath, branchName));

    public async Task CreateBranchAsync(string repoPath, string branchName) 
        => await ExecuteSafeAsync(s => s.CreateBranchAsync(repoPath, branchName));

    public bool IsRepository(string path)
    {
        if (_useFallback) return _fallback.IsRepository(path);
        
        try
        {
            return GetPrimary().IsRepository(path);
        }
        catch (Exception ex) when (IsNativeError(ex))
        {
            _useFallback = true;
            return _fallback.IsRepository(path);
        }
    }

    private async Task<T> ExecuteSafeAsync<T>(Func<IGitService, Task<T>> action)
    {
        if (_useFallback) return await action(_fallback);

        try
        {
            return await action(GetPrimary());
        }
        catch (Exception ex) when (IsNativeError(ex))
        {
            _useFallback = true;
            return await action(_fallback);
        }
    }

    private async Task ExecuteSafeAsync(Func<IGitService, Task> action)
    {
        if (_useFallback)
        {
            await action(_fallback);
            return;
        }

        try
        {
            await action(GetPrimary());
        }
        catch (Exception ex) when (IsNativeError(ex))
        {
            _useFallback = true;
            await action(_fallback);
        }
    }

    private static bool IsNativeError(Exception ex)
    {
        if (ex is TypeInitializationException or DllNotFoundException) return true;
        
        if (ex is AggregateException aggEx)
        {
            return aggEx.InnerExceptions.Any(IsNativeError);
        }

        if (ex.InnerException is not null) return IsNativeError(ex.InnerException);
        return false;
    }
}
