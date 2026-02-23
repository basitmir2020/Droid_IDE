using DroidIDE.App.ViewModels;

namespace DroidIDE.App.Views.Shell;

public partial class MainShell : ContentPage
{
    public MainShell(MainShellViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        AddVerticalResize();
        AddHorizontalResize();
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
                    startWidth = ExplorerColumn.Width.Value;
                    break;

                case GestureStatus.Running:
                    double newWidth = startWidth + e.TotalX;

                    if (newWidth > 150 && newWidth < 500)
                    {
                        ExplorerColumn.Width = new GridLength(newWidth);
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
    
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width < 800)
        {
            // hide explorer
        }
    }
    
    
}