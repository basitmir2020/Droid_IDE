using System.Windows.Input;

namespace DroidIDE.App.ViewModels;

/// <summary>
/// ViewModel for the main application shell — manages panel visibility and layout state.
/// </summary>
public class MainShellViewModel : BaseViewModel
{
    private readonly ExplorerViewModel _explorerViewModel;
    private readonly EditorViewModel _editorViewModel;
    private readonly TerminalViewModel _terminalViewModel;

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

    public ExplorerViewModel Explorer => _explorerViewModel;
    public EditorViewModel Editor => _editorViewModel;
    public TerminalViewModel Terminal => _terminalViewModel;

    public MainShellViewModel(
        ExplorerViewModel explorerViewModel,
        EditorViewModel editorViewModel,
        TerminalViewModel terminalViewModel)
    {
        _explorerViewModel = explorerViewModel;
        _editorViewModel = editorViewModel;
        _terminalViewModel = terminalViewModel;

        Title = "DroidIDE";

        ToggleExplorerCommand = new RelayCommand(() => IsExplorerVisible = !IsExplorerVisible);
        ToggleBottomPanelCommand = new RelayCommand(() => IsBottomPanelVisible = !IsBottomPanelVisible);
        ShowPanelCommand = new RelayCommand(param =>
        {
            if (param is string panelName)
            {
                ActivePanel = panelName;
                IsExplorerVisible = true;
            }
        });

        // Wire explorer file-open events to the editor
        _explorerViewModel.FileOpenRequested += tab =>
        {
            _editorViewModel.OpenTab(tab);
            StatusText = $"Opened {tab.DisplayName}";
        };
    }
}