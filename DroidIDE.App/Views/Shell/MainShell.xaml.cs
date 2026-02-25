using DroidIDE.App.ViewModels;
using DroidIDE.Editor.Monaco;

namespace DroidIDE.App.Views.Shell;

/// <summary>
/// Code-behind for the main shell layout. Handles panel resize gestures,
/// sidebar column collapse, Monaco bridge injection, and terminal initialization.
/// </summary>
public partial class MainShell : ContentPage
{
    private readonly MainShellViewModel _viewModel;

    public MainShell(MainShellViewModel viewModel, MonacoEditorBridge bridge)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Wire up Monaco bridge into the EditorHostView
        EditorHost.SetBridge(bridge);

        AddVerticalResize();
        AddHorizontalResize();

        // Listen for sidebar visibility changes to collapse/expand column
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Auto-create initial terminal session after page loads
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        // Create a default terminal session so the terminal is usable immediately
        try
        {
            await _viewModel.Terminal.CreateSessionAsync(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to create initial terminal session: {ex.Message}");
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainShellViewModel.IsExplorerVisible))
        {
            // Collapse sidebar column when hidden, expand when shown
            if (_viewModel.IsExplorerVisible)
            {
                SidebarColumn.Width = new GridLength(260);
                SplitterColumn.Width = new GridLength(6);
            }
            else
            {
                SidebarColumn.Width = new GridLength(0);
                SplitterColumn.Width = new GridLength(0);
            }
        }
    }

    private void AddVerticalResize()
    {
        var panGesture = new PanGestureRecognizer();
        double startWidth = 0;

        panGesture.PanUpdated += (s, e) =>
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    startWidth = SidebarColumn.Width.Value;
                    break;

                case GestureStatus.Running:
                    double newWidth = startWidth + e.TotalX;

                    if (newWidth > 150 && newWidth < 500)
                    {
                        SidebarColumn.Width = new GridLength(newWidth);
                    }
                    break;
            }
        };

        VerticalSplitter.GestureRecognizers.Add(panGesture);
    }
    
    private void AddHorizontalResize()
    {
        var panGesture = new PanGestureRecognizer();
        double startHeight = 0;

        panGesture.PanUpdated += (s, e) =>
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    startHeight = BottomRow.Height.Value;
                    break;

                case GestureStatus.Running:
                    double newHeight = startHeight - e.TotalY;

                    if (newHeight > 120 && newHeight < 500)
                    {
                        BottomRow.Height = new GridLength(newHeight);
                    }
                    break;
            }
        };

        HorizontalSplitter.GestureRecognizers.Add(panGesture);
    }
    

}