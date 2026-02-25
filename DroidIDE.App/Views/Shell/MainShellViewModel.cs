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

    public ExplorerViewModel Explorer => _explorerViewModel;
    public EditorViewModel Editor => _editorViewModel;
    public TerminalViewModel Terminal => _terminalViewModel;
    public SearchViewModel Search => _searchViewModel;
    public GitViewModel Git => _gitViewModel;
    public SettingsViewModel Settings => _settingsViewModel;

    public MainShellViewModel(
        ExplorerViewModel explorerViewModel,
        EditorViewModel editorViewModel,
        TerminalViewModel terminalViewModel,
        SearchViewModel searchViewModel,
        GitViewModel gitViewModel,
        SettingsViewModel settingsViewModel,
        IGitService gitService,
        IDotnetCli dotnetCli)
    {
        _explorerViewModel = explorerViewModel;
        _editorViewModel = editorViewModel;
        _terminalViewModel = terminalViewModel;
        _searchViewModel = searchViewModel;
        _gitViewModel = gitViewModel;
        _settingsViewModel = settingsViewModel;
        _gitService = gitService;
        _dotnetCli = dotnetCli;

        Title = "DroidIDE";

        ToggleExplorerCommand = new RelayCommand(() => IsExplorerVisible = !IsExplorerVisible);
        ToggleBottomPanelCommand = new RelayCommand(() => IsBottomPanelVisible = !IsBottomPanelVisible);
        
        OpenFolderCommand = new RelayCommand(() => _explorerViewModel.OpenFolderCommand.Execute(null));
        
        CloneRepoCommand = new AsyncRelayCommand(async () =>
        {
            var url = await Shell.Current.DisplayPromptAsync("Clone Repository", "Enter Git repository URL:", "Clone", "Cancel", "https://github.com/...");
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
                StatusText = "Repository cloned successfully";
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Clone Failed", ex.Message, "OK");
                StatusText = "Clone failed";
            }
            finally
            {
                IsBusy = false;
            }
        });

        NewProjectCommand = new AsyncRelayCommand(async () =>
        {
            var template = await Shell.Current.DisplayActionSheet("Select Project Template", "Cancel", null, "console", "classlib", "webapi", "maui");
            if (template == "Cancel" || string.IsNullOrEmpty(template)) return;

            var name = await Shell.Current.DisplayPromptAsync("New Project", "Enter project name:", "Create", "Cancel", "MyProject");
            if (string.IsNullOrEmpty(name)) return;

            StatusText = "Select output folder...";
            var targetPath = await PickFolderAsync();
            if (string.IsNullOrEmpty(targetPath))
            {
                StatusText = "Ready";
                return;
            }

            try
            {
                IsBusy = true;
                StatusText = $"Creating {template} project '{name}'...";
                await _dotnetCli.NewAsync(template, targetPath, name);
                
                var projectPath = Path.Combine(targetPath, name);
                await _explorerViewModel.OpenFolderAsync(projectPath);
                StatusText = $"Project '{name}' created successfully";
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Creation Failed", ex.Message, "OK");
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

            // Also set search root path when files are opened
            if (!string.IsNullOrEmpty(_explorerViewModel.CurrentPath))
                _searchViewModel.SearchRootPath = _explorerViewModel.CurrentPath;
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
}