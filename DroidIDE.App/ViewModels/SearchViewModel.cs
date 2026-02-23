using System.Collections.ObjectModel;
using System.Windows.Input;
using DroidIDE.App.Services;

namespace DroidIDE.App.ViewModels;

/// <summary>
/// ViewModel for the Search panel — drives find-in-files functionality.
/// </summary>
public class SearchViewModel : BaseViewModel
{
    private readonly SearchService _searchService;
    private CancellationTokenSource? _searchCts;

    public SearchViewModel(SearchService searchService)
    {
        _searchService = searchService;
        Title = "Search";

        SearchCommand = new AsyncRelayCommand(ExecuteSearchAsync);
        ClearCommand = new RelayCommand(ClearResults);

        _searchService.FilesScanned += count =>
            StatusText = $"Scanned {count} files...";
    }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    private bool _caseSensitive;
    public bool CaseSensitive
    {
        get => _caseSensitive;
        set => SetProperty(ref _caseSensitive, value);
    }

    private string _fileFilter = string.Empty;
    public string FileFilter
    {
        get => _fileFilter;
        set => SetProperty(ref _fileFilter, value);
    }

    private string _statusText = "Ready";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private string? _searchRootPath;
    public string? SearchRootPath
    {
        get => _searchRootPath;
        set => SetProperty(ref _searchRootPath, value);
    }

    public ObservableCollection<SearchResultGroup> ResultGroups { get; } = [];

    public ICommand SearchCommand { get; }
    public ICommand ClearCommand { get; }

    /// <summary>Fired when user wants to open a search result in the editor.</summary>
    public event Action<string, int>? OpenFileRequested;

    public void OpenResult(SearchResult result)
    {
        OpenFileRequested?.Invoke(result.FilePath, result.LineNumber);
    }

    private async Task ExecuteSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || string.IsNullOrWhiteSpace(SearchRootPath))
            return;

        // Cancel any previous search
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        IsBusy = true;
        ResultGroups.Clear();
        StatusText = "Searching...";

        try
        {
            var extensions = string.IsNullOrWhiteSpace(FileFilter)
                ? null
                : FileFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var results = await _searchService.SearchInFilesAsync(
                SearchRootPath, SearchQuery, CaseSensitive, extensions, _searchCts.Token);

            // Group by file
            var groups = results
                .GroupBy(r => r.RelativePath)
                .Select(g => new SearchResultGroup
                {
                    FileName = g.Key,
                    MatchCount = g.Count(),
                    Results = new ObservableCollection<SearchResult>(g.ToList())
                });

            foreach (var group in groups)
                ResultGroups.Add(group);

            StatusText = $"{results.Count} results in {ResultGroups.Count} files";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Search cancelled";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearResults()
    {
        ResultGroups.Clear();
        SearchQuery = string.Empty;
        StatusText = "Ready";
    }
}

/// <summary>
/// Groups search results by file for display.
/// </summary>
public class SearchResultGroup
{
    public string FileName { get; set; } = string.Empty;
    public int MatchCount { get; set; }
    public ObservableCollection<SearchResult> Results { get; set; } = [];
}
