using DroidIDE.App.Views.Shell;

namespace DroidIDE.App;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
     {
        var mainShell = _serviceProvider.GetRequiredService<MainShell>();
        return new Window(mainShell);
    }
}
