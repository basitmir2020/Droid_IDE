using DroidIDE.Core.Enums;
using DroidIDE.Core.Interfaces;

namespace DroidIDE.Git.GitService;

/// <summary>
/// CLI-based implementation of <see cref="IGitService"/>.
/// Useful as a fallback on platforms where LibGit2Sharp native libraries fail to load (e.g., Android).
/// </summary>
public class CliGitService : IGitService
{
    private readonly IProcessManager _processManager;

    public CliGitService(IProcessManager processManager)
    {
        _processManager = processManager;
    }

    public async Task InitAsync(string path)
    {
        await RunGitAsync("init", path);
    }

    public async Task CloneAsync(string url, string targetPath, IProgress<string>? progress = null)
    {
        // git clone <url> <targetPath>
        // Note: Progress reporting is harder with CLI but we can at least report that it's starting.
        progress?.Report("Starting clone...");
        await RunGitAsync($"clone \"{url}\" \"{targetPath}\"", Path.GetDirectoryName(targetPath) ?? ".");
    }

    public async Task StageAsync(string repoPath, IEnumerable<string> filePaths)
    {
        var paths = string.Join(" ", filePaths.Select(p => $"\"{p}\""));
        await RunGitAsync($"add {paths}", repoPath);
    }

    public async Task StageAllAsync(string repoPath)
    {
        await RunGitAsync("add .", repoPath);
    }

    public async Task CommitAsync(string repoPath, string message, string authorName, string authorEmail)
    {
        // Set config for this repo if not set
        await RunGitAsync($"config user.name \"{authorName}\"", repoPath);
        await RunGitAsync($"config user.email \"{authorEmail}\"", repoPath);
        await RunGitAsync($"commit -m \"{message}\"", repoPath);
    }

    public async Task PushAsync(string repoPath, string remoteName = "origin", string branchName = "main")
    {
        await RunGitAsync($"push {remoteName} {branchName}", repoPath);
    }

    public async Task PullAsync(string repoPath, string remoteName = "origin")
    {
        await RunGitAsync($"pull {remoteName}", repoPath);
    }

    public async Task<Dictionary<string, GitFileStatus>> GetStatusAsync(string repoPath)
    {
        var result = await _processManager.RunAsync("git", "status --porcelain", repoPath);
        var statusMap = new Dictionary<string, GitFileStatus>();

        if (!result.Success) return statusMap;

        var lines = result.StandardOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.Length < 3) continue;
            var statusCode = line.Substring(0, 2).Trim();
            var filePath = line.Substring(3).Trim();

            statusMap[filePath] = MapCliStatus(statusCode);
        }

        return statusMap;
    }

    public async Task<List<string>> GetBranchesAsync(string repoPath)
    {
        var result = await _processManager.RunAsync("git", "branch --format=\"%(refname:short)\"", repoPath);
        if (!result.Success) return new List<string>();

        return result.StandardOutput
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim())
            .ToList();
    }

    public async Task<string> GetCurrentBranchAsync(string repoPath)
    {
        var result = await _processManager.RunAsync("git", "rev-parse --abbrev-ref HEAD", repoPath);
        return result.Success ? result.StandardOutput.Trim() : "main";
    }

    public async Task CheckoutAsync(string repoPath, string branchName)
    {
        await RunGitAsync($"checkout {branchName}", repoPath);
    }

    public async Task CreateBranchAsync(string repoPath, string branchName)
    {
        await RunGitAsync($"checkout -b {branchName}", repoPath);
    }

    public bool IsRepository(string path)
    {
        // Quick check without async overhead
        return Directory.Exists(Path.Combine(path, ".git"));
    }

    private async Task RunGitAsync(string args, string workingDir)
    {
        var result = await _processManager.RunAsync("git", args, workingDir);
        if (!result.Success)
        {
            throw new Exception($"Git command failed: git {args}\nError: {result.StandardError}");
        }
    }

    private static GitFileStatus MapCliStatus(string statusCode)
    {
        return statusCode switch
        {
            "M" or " M" or "MM" => GitFileStatus.Modified,
            "A" or " A"          => GitFileStatus.Added,
            "D" or " D"          => GitFileStatus.Deleted,
            "R" or " R"          => GitFileStatus.Renamed,
            "??"                 => GitFileStatus.Untracked,
            "U" or "AA" or "DD"  => GitFileStatus.Conflicted,
            _                    => GitFileStatus.Unmodified
        };
    }
}
