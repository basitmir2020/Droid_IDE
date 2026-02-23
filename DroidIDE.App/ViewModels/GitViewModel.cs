using System.Collections.ObjectModel;
using System.Windows.Input;
using DroidIDE.Core.Enums;
using DroidIDE.Core.Interfaces;
using DroidIDE.Git.BranchManager;
using DroidIDE.Git.RepositoryManager;

namespace DroidIDE.App.ViewModels;

/// <summary>
/// ViewModel for the Git source control panel — branch management, status, and commit.
/// </summary>
public class GitViewModel : BaseViewModel
{
    private readonly IGitService _gitService;
    private readonly BranchManager _branchManager;
    private readonly RepositoryManager _repoManager;

    public GitViewModel(IGitService gitService, BranchManager branchManager, RepositoryManager repoManager)
    {
        _gitService = gitService;
        _branchManager = branchManager;
        _repoManager = repoManager;

        Title = "Source Control";

        RefreshCommand = new AsyncRelayCommand(RefreshStatusAsync);
        CommitCommand = new AsyncRelayCommand(CommitAsync);
        StageAllCommand = new AsyncRelayCommand(StageAllAsync);
        PullCommand = new AsyncRelayCommand(PullAsync);
        PushCommand = new AsyncRelayCommand(PushAsync);
        SyncCommand = new AsyncRelayCommand(SyncAsync);
        CreateBranchCommand = new AsyncRelayCommand(CreateBranchAsync);

        _repoManager.RepositoryOpened += path =>
        {
            IsRepoOpen = true;
            _ = RefreshStatusAsync();
        };

        _branchManager.BranchChanged += branch =>
        {
            CurrentBranch = branch;
        };
    }

    private bool _isRepoOpen;
    public bool IsRepoOpen
    {
        get => _isRepoOpen;
        set => SetProperty(ref _isRepoOpen, value);
    }

    private string _currentBranch = string.Empty;
    public string CurrentBranch
    {
        get => _currentBranch;
        set => SetProperty(ref _currentBranch, value);
    }

    private string _commitMessage = string.Empty;
    public string CommitMessage
    {
        get => _commitMessage;
        set => SetProperty(ref _commitMessage, value);
    }

    private string _authorName = string.Empty;
    public string AuthorName
    {
        get => _authorName;
        set => SetProperty(ref _authorName, value);
    }

    private string _authorEmail = string.Empty;
    public string AuthorEmail
    {
        get => _authorEmail;
        set => SetProperty(ref _authorEmail, value);
    }

    private string _newBranchName = string.Empty;
    public string NewBranchName
    {
        get => _newBranchName;
        set => SetProperty(ref _newBranchName, value);
    }

    private string _selectedBranch = string.Empty;
    public string SelectedBranch
    {
        get => _selectedBranch;
        set
        {
            if (SetProperty(ref _selectedBranch, value) && !string.IsNullOrEmpty(value))
            {
                _ = SwitchBranchAsync(value);
            }
        }
    }

    public ObservableCollection<GitFileEntry> ChangedFiles { get; } = [];
    public ObservableCollection<string> Branches { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand CommitCommand { get; }
    public ICommand StageAllCommand { get; }
    public ICommand PullCommand { get; }
    public ICommand PushCommand { get; }
    public ICommand SyncCommand { get; }
    public ICommand CreateBranchCommand { get; }

    /// <summary>
    /// Initialize the Git panel for a given repository path.
    /// </summary>
    public async Task InitializeAsync(string repoPath)
    {
        if (_gitService.IsRepository(repoPath))
        {
            _repoManager.OpenRepository(repoPath);
            CurrentBranch = await _branchManager.GetCurrentBranchAsync(repoPath);
            await RefreshStatusAsync();
            await RefreshBranchesAsync();
        }
    }

    private async Task RefreshStatusAsync()
    {
        if (_repoManager.CurrentRepoPath is null) return;

        IsBusy = true;
        try
        {
            var status = await _repoManager.RefreshStatusAsync();
            ChangedFiles.Clear();
            foreach (var (path, fileStatus) in status)
            {
                ChangedFiles.Add(new GitFileEntry
                {
                    FilePath = path,
                    FileName = Path.GetFileName(path),
                    Status = fileStatus,
                    StatusLabel = GetStatusLabel(fileStatus),
                    StatusColor = GetStatusColor(fileStatus)
                });
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshBranchesAsync()
    {
        if (_repoManager.CurrentRepoPath is null) return;

        var branches = await _branchManager.GetAllBranchesAsync(_repoManager.CurrentRepoPath);
        Branches.Clear();
        foreach (var branch in branches)
            Branches.Add(branch);
    }

    private async Task CommitAsync()
    {
        if (_repoManager.CurrentRepoPath is null || string.IsNullOrWhiteSpace(CommitMessage))
            return;

        IsBusy = true;
        try
        {
            await _repoManager.QuickCommitAsync(CommitMessage, AuthorName, AuthorEmail);
            CommitMessage = string.Empty;
            await RefreshStatusAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StageAllAsync()
    {
        if (_repoManager.CurrentRepoPath is null) return;
        await _gitService.StageAllAsync(_repoManager.CurrentRepoPath);
        await RefreshStatusAsync();
    }

    private async Task PullAsync()
    {
        if (_repoManager.CurrentRepoPath is null) return;
        IsBusy = true;
        try { await _gitService.PullAsync(_repoManager.CurrentRepoPath); }
        finally { IsBusy = false; }
        await RefreshStatusAsync();
    }

    private async Task PushAsync()
    {
        if (_repoManager.CurrentRepoPath is null) return;
        IsBusy = true;
        try { await _gitService.PushAsync(_repoManager.CurrentRepoPath); }
        finally { IsBusy = false; }
    }

    private async Task SyncAsync()
    {
        if (_repoManager.CurrentRepoPath is null) return;
        IsBusy = true;
        try { await _repoManager.SyncAsync(); }
        finally { IsBusy = false; }
        await RefreshStatusAsync();
    }

    private async Task CreateBranchAsync()
    {
        if (_repoManager.CurrentRepoPath is null || string.IsNullOrWhiteSpace(NewBranchName))
            return;

        await _branchManager.CreateAndCheckoutAsync(_repoManager.CurrentRepoPath, NewBranchName);
        NewBranchName = string.Empty;
        await RefreshBranchesAsync();
    }

    private async Task SwitchBranchAsync(string branchName)
    {
        if (_repoManager.CurrentRepoPath is null) return;
        await _branchManager.SwitchBranchAsync(_repoManager.CurrentRepoPath, branchName);
        await RefreshStatusAsync();
    }

    private static string GetStatusLabel(GitFileStatus status) => status switch
    {
        GitFileStatus.Modified => "M",
        GitFileStatus.Added => "A",
        GitFileStatus.Deleted => "D",
        GitFileStatus.Renamed => "R",
        GitFileStatus.Untracked => "U",
        GitFileStatus.Conflicted => "C",
        _ => "?"
    };

    private static string GetStatusColor(GitFileStatus status) => status switch
    {
        GitFileStatus.Modified => "#E2C08D",
        GitFileStatus.Added => "#73C991",
        GitFileStatus.Deleted => "#C74E39",
        GitFileStatus.Renamed => "#73C991",
        GitFileStatus.Untracked => "#73C991",
        GitFileStatus.Conflicted => "#E51400",
        _ => "#CCCCCC"
    };
}

/// <summary>
/// Represents a changed file in the Git status view.
/// </summary>
public class GitFileEntry
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public GitFileStatus Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusColor { get; set; } = "#CCCCCC";
}
