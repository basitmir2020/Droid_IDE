using System.Windows.Input;
using DroidIDE.Core.Interfaces;

namespace DroidIDE.App.ViewModels;

/// <summary>
/// ViewModel for the main application shell — manages panel visibility, layout state,
/// and exposes child ViewModels for the sidebar and bottom panels.
/// </summary>
public class MainShellViewModel : BaseViewModel
{
    private readonly ExplorerViewModel _explorerViewModel;
    private readonly EditorViewModel _editorViewModel;
    private readonly TerminalViewModel _terminalViewModel;
    private readonly SearchViewModel _searchViewModel;
    private readonly GitViewModel _gitViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly BuildViewModel _buildViewModel;
    private readonly IGitService _gitService;
    private readonly IDotnetCli _dotnetCli;

    private bool _isExplorerVisible = true;
    public bool IsExplorerVisible
    {
        get => _isExplorerVisible;
        set => SetProperty(ref _isExplorerVisible, value);
    }

    private bool _isBottomPanelVisible = true;
    public bool IsBottomPanelVisible
    {
        get => _isBottomPanelVisible;
        set => SetProperty(ref _isBottomPanelVisible, value);
    }

    private string _activePanel = "Explorer";
    public string ActivePanel
    {
        get => _activePanel;
        set => SetProperty(ref _activePanel, value);
    }

    private string _statusText = "Ready";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public ICommand ToggleExplorerCommand { get; }
    public ICommand ToggleBottomPanelCommand { get; }
    public ICommand ShowPanelCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand CloneRepoCommand { get; }
    public ICommand NewProjectCommand { get; }

    public ExplorerViewModel Explorer  => _explorerViewModel;
    public EditorViewModel Editor      => _editorViewModel;
    public TerminalViewModel Terminal  => _terminalViewModel;
    public SearchViewModel Search      => _searchViewModel;
    public GitViewModel Git            => _gitViewModel;
    public SettingsViewModel Settings  => _settingsViewModel;
    public BuildViewModel Build        => _buildViewModel;

    public MainShellViewModel(
        ExplorerViewModel explorerViewModel,
        EditorViewModel editorViewModel,
        TerminalViewModel terminalViewModel,
        SearchViewModel searchViewModel,
        GitViewModel gitViewModel,
        SettingsViewModel settingsViewModel,
        BuildViewModel buildViewModel,
        IGitService gitService,
        IDotnetCli dotnetCli)
    {
        _explorerViewModel  = explorerViewModel;
        _editorViewModel    = editorViewModel;
        _terminalViewModel  = terminalViewModel;
        _searchViewModel    = searchViewModel;
        _gitViewModel       = gitViewModel;
        _settingsViewModel  = settingsViewModel;
        _buildViewModel     = buildViewModel;
        _gitService         = gitService;
        _dotnetCli          = dotnetCli;

        Title = "DroidIDE";

        ToggleExplorerCommand = new RelayCommand(() => IsExplorerVisible = !IsExplorerVisible);
        ToggleBottomPanelCommand = new RelayCommand(() => IsBottomPanelVisible = !IsBottomPanelVisible);
        
        OpenFolderCommand = new RelayCommand(() => _explorerViewModel.OpenFolderCommand.Execute(null));
        
        CloneRepoCommand = new AsyncRelayCommand(async () =>
        {
            var page = Application.Current?.MainPage;
            if (page is null) return;

            var url = await page.DisplayPromptAsync("Clone Repository", "Enter Git repository URL:", "Clone", "Cancel", "https://github.com/...");
            if (string.IsNullOrEmpty(url)) return;

            StatusText = "Select target folder for clone...";
            var targetPath = await PickFolderAsync();
            if (string.IsNullOrEmpty(targetPath))
            {
                StatusText = "Ready";
                return;
            }

            try
            {
                IsBusy = true;
                StatusText = $"Cloning {url}...";
                await _gitService.CloneAsync(url, targetPath);
                await _explorerViewModel.OpenFolderAsync(targetPath);
                await _gitViewModel.InitializeAsync(targetPath);
                _buildViewModel.SetProjectPath(targetPath);
                StatusText = "Repository cloned successfully";
            }
            catch (Exception ex)
            {
                await page.DisplayAlert("Clone Failed", ex.Message, "OK");
                StatusText = "Clone failed";
            }
            finally
            {
                IsBusy = false;
            }
        });

        NewProjectCommand = new AsyncRelayCommand(async () =>
        {
            var page = Application.Current?.MainPage;
            if (page is null) return;

            var template = await page.DisplayActionSheet("Select Project Template", "Cancel", null, "console", "classlib", "webapi", "maui");
            if (template == "Cancel" || string.IsNullOrEmpty(template)) return;

            var name = await page.DisplayPromptAsync("New Project", "Enter project name:", "Create", "Cancel", "MyProject");
            if (string.IsNullOrEmpty(name)) return;

            // Auto-select default projects directory
            var projectsRoot = Path.Combine(FileSystem.AppDataDirectory, "Projects");
            Directory.CreateDirectory(projectsRoot);
            var projectPath = Path.Combine(projectsRoot, name);

            try
            {
                IsBusy = true;
                StatusText = $"Creating {template} project '{name}'...";

                // Create project files directly — no dotnet CLI needed
                await Task.Run(() => Services.ProjectScaffolder.Create(template, projectPath, name));

                await _explorerViewModel.OpenFolderAsync(projectPath);
                
                // Ensure sidebar is visible and active
                ActivePanel = "Explorer";
                IsExplorerVisible = true;

                // Automatically open the main file to show the project is "opened"
                string mainFile = template.ToLowerInvariant() switch
                {
                    "console" or "webapi" or "maui" => "Program.cs",
                    "classlib" => "Class1.cs",
                    _ => "Program.cs"
                };
                var mainFilePath = Path.Combine(projectPath, mainFile);
                if (File.Exists(mainFilePath))
                {
                    await _editorViewModel.OpenFileForEditorAsync(mainFilePath);
                }

                _ = _gitViewModel.InitializeAsync(projectPath);
                _buildViewModel.SetProjectPath(projectPath);
                StatusText = $"Project '{name}' opened";
            }
            catch (Exception ex)
            {
                await page.DisplayAlert("Creation Failed", ex.Message, "OK");
                StatusText = "Project creation failed";
            }
            finally
            {
                IsBusy = false;
            }
        });

        ShowPanelCommand = new RelayCommand(param =>
        {
            if (param is string panelName)
            {
                // If tapping the same panel, toggle sidebar visibility
                if (ActivePanel == panelName && IsExplorerVisible)
                {
                    IsExplorerVisible = false;
                    return;
                }

                ActivePanel = panelName;
                IsExplorerVisible = true;
            }
        });

        // Wire explorer file-open events to the editor
        _explorerViewModel.FileOpenRequested += tab =>
        {
            _editorViewModel.OpenTab(tab);
            StatusText = $"Opened {tab.DisplayName}";

            // Set search root path when files are opened
            if (!string.IsNullOrEmpty(_explorerViewModel.CurrentPath))
                _searchViewModel.SearchRootPath = _explorerViewModel.CurrentPath;

            // Initialize Git panel for the opened folder
            if (!string.IsNullOrEmpty(_explorerViewModel.CurrentPath))
                _ = _gitViewModel.InitializeAsync(_explorerViewModel.CurrentPath);
        };

        // Wire search result navigation to the editor
        _searchViewModel.OpenFileRequested += (filePath, lineNumber) =>
        {
            _ = OpenSearchResultAsync(filePath, lineNumber);
        };
    }

    private async Task<string?> PickFolderAsync()
    {
        try
        {
            var fileResult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select any file in the target folder"
            });

            if (fileResult != null)
            {
                return Path.GetDirectoryName(fileResult.FullPath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PickFolderAsync failed: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Opens a file from a search result and navigates to the specified line.
    /// </summary>
    private async Task OpenSearchResultAsync(string filePath, int lineNumber)
    {
        try
        {
            var tab = await _editorViewModel.OpenFileForEditorAsync(filePath);
            if (tab is not null)
                StatusText = $"Opened {tab.DisplayName}:{lineNumber}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OpenSearchResult failed: {ex.Message}");
        }
    }
}
