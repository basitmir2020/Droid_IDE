using DroidIDE.App.Services;
using DroidIDE.App.ViewModels;
using DroidIDE.App.Views.Shell;
using DroidIDE.Core.Interfaces;
using DroidIDE.Editor.Diagnostics;
using DroidIDE.Editor.LanguageServer;
using DroidIDE.Editor.Monaco;
using DroidIDE.Editor.Refactoring;
using DroidIDE.Git.BranchManager;
using DroidIDE.Git.GitService;
using DroidIDE.Git.RepositoryManager;
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

        // ── Git Services ──
        builder.Services.AddSingleton<IGitService, GitService>();
        builder.Services.AddSingleton<BranchManager>();
        builder.Services.AddSingleton<RepositoryManager>();

        // ── Cross-Cutting Services ──
        builder.Services.AddSingleton<AppLogger>();
        builder.Services.AddSingleton<GlobalExceptionHandler>();
        builder.Services.AddSingleton<ThemeService>();

        // ── App Services ──
        builder.Services.AddSingleton<IEditorService, EditorService>();
        builder.Services.AddSingleton<CommandPaletteService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<KeyboardShortcutService>();
        builder.Services.AddSingleton<SearchService>();
        builder.Services.AddSingleton<FileWatcherService>();
        builder.Services.AddSingleton<AppStateService>();

        // ── ViewModels ──
        builder.Services.AddSingleton<MainShellViewModel>();
        builder.Services.AddSingleton<ExplorerViewModel>();
        builder.Services.AddSingleton<EditorViewModel>();
        builder.Services.AddSingleton<TerminalViewModel>();
        builder.Services.AddSingleton<SearchViewModel>();
        builder.Services.AddSingleton<GitViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();

        // ── Pages / Views ──
        builder.Services.AddSingleton<MainShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

