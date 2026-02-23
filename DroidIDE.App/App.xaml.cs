using DroidIDE.App.Services;
using DroidIDE.App.Views.Shell;

namespace DroidIDE.App;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();

        // Register global exception handler
        var exceptionHandler = _serviceProvider.GetRequiredService<GlobalExceptionHandler>();
        exceptionHandler.Register();

        // Apply saved theme
        var themeService = _serviceProvider.GetRequiredService<ThemeService>();
        themeService.ApplySavedTheme();

        // Log startup
        var logger = _serviceProvider.GetRequiredService<AppLogger>();
        logger.LogInfo("DroidIDE application started.");
    }

    protected override Window CreateWindow(IActivationState? activationState)
     {
        var mainShell = _serviceProvider.GetRequiredService<MainShell>();
        return new Window(mainShell);
    }
}
