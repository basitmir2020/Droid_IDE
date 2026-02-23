namespace DroidIDE.App.Services;

/// <summary>
/// Global exception handler — catches unhandled exceptions across the app,
/// logs them, and shows a user-friendly error dialog instead of crashing.
/// </summary>
public class GlobalExceptionHandler
{
    private readonly AppLogger _logger;

    public GlobalExceptionHandler(AppLogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Register all unhandled exception hooks. Call once at app startup.
    /// </summary>
    public void Register()
    {
        // .NET unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // Task scheduler unobserved exceptions
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        _logger.LogError("Unhandled Exception", ex);

        if (e.IsTerminating)
        {
            _logger.LogError("Application is terminating due to unhandled exception.");
        }

        ShowErrorDialog(ex);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger.LogError("Unobserved Task Exception", e.Exception);
        e.SetObserved(); // Prevent crash
        ShowErrorDialog(e.Exception);
    }

    private static void ShowErrorDialog(Exception? exception)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                var page = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page is not null)
                {
                    var message = exception?.Message ?? "An unexpected error occurred.";
                    await page.DisplayAlertAsync(
                        "Error",
                        $"Something went wrong:\n\n{message}\n\nThe error has been logged.",
                        "OK");
                }
            }
            catch
            {
                // Swallow — we're already in error handling
            }
        });
    }
}
