using DroidIDE.App.Services;
using DroidIDE.App.ViewModels;
using DroidIDE.App.Views.Shell;
using DroidIDE.Core.Interfaces;
using DroidIDE.Editor.Diagnostics;
using DroidIDE.Editor.LanguageServer;
using DroidIDE.Editor.Monaco;
using DroidIDE.Editor.Refactoring;
using DroidIDE.Infrastructure.FileSystem;
using DroidIDE.Infrastructure.Process;
using DroidIDE.ProjectSystem.ProjectParser;
using DroidIDE.ProjectSystem.SolutionParser;
using DroidIDE.Runtime.Build;
using DroidIDE.Runtime.DotnetManager;
using DroidIDE.Runtime.Run;
using DroidIDE.Runtime.ServiceManager;
using DroidIDE.Terminal.ProcessSession;
using Microsoft.Extensions.Logging;

namespace DroidIDE.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ── Infrastructure Services ──
        builder.Services.AddSingleton<IFileSystemService, FileSystemService>();
        builder.Services.AddSingleton<IProcessManager, ProcessManager>();
        builder.Services.AddSingleton<ITerminalService, ProcessSessionManager>();

        // ── Project System Services ──
        builder.Services.AddSingleton<IProjectParser, ProjectParser>();
        builder.Services.AddSingleton<ISolutionParser, SolutionParser>();
        builder.Services.AddSingleton<ProjectSystem.NuGetManager.NuGetManager>();

        // ── Runtime Services ──
        builder.Services.AddSingleton<IDotnetCli, DotnetCliService>();
        builder.Services.AddSingleton<BuildService>();
        builder.Services.AddSingleton<RunService>();
        builder.Services.AddSingleton<ServiceManager>();

        // ── Editor Services ──
        builder.Services.AddSingleton<MonacoEditorBridge>();
        builder.Services.AddSingleton<RoslynLanguageService>();
        builder.Services.AddSingleton<DiagnosticService>();
        builder.Services.AddSingleton<RefactoringService>();

        // ── App Services ──
        builder.Services.AddSingleton<IEditorService, EditorService>();

        // ── ViewModels ──
        builder.Services.AddSingleton<MainShellViewModel>();
        builder.Services.AddSingleton<ExplorerViewModel>();
        builder.Services.AddSingleton<EditorViewModel>();
        builder.Services.AddSingleton<TerminalViewModel>();

        // ── Pages / Views ──
        builder.Services.AddSingleton<MainShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
