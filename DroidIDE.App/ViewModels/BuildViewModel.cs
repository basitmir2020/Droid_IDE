using System.Collections.ObjectModel;
using System.Windows.Input;
using DroidIDE.Runtime.Build;
using DroidIDE.Runtime.Run;

namespace DroidIDE.App.ViewModels;

/// <summary>
/// ViewModel for build and run operations. Exposes commands for Build, Run, Stop, and Clean
/// and streams output lines to <see cref="BuildOutput"/> for display in the Output bottom panel.
/// </summary>
public class BuildViewModel : BaseViewModel
{
    private readonly BuildService _buildService;
    private readonly RunService _runService;

    private string? _currentProjectPath;
    private string _buildStatus = "Ready";
    private bool _isRunning;

    /// <summary>Streamed output from build and run operations.</summary>
    public ObservableCollection<string> BuildOutput { get; } = [];

    /// <summary>Raised when a build, run, or clean operation starts.</summary>
    public event Action? BuildStarted;

    /// <summary>Current status text (Ready / Building / Running / Failed / Success).</summary>
    public string BuildStatus
    {
        get => _buildStatus;
        set => SetProperty(ref _buildStatus, value);
    }

    /// <summary>True while a process (build or run) is executing.</summary>
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            SetProperty(ref _isRunning, value);
            OnPropertyChanged(nameof(CanBuildOrRun));
        }
    }

    /// <summary>Inverse of IsRunning — used to enable/disable toolbar buttons.</summary>
    public bool CanBuildOrRun => !_isRunning;

    public ICommand BuildCommand       { get; }
    public ICommand RunCommand         { get; }
    public ICommand StopCommand        { get; }
    public ICommand CleanCommand       { get; }
    public ICommand ClearOutputCommand { get; }

    public BuildViewModel(BuildService buildService, RunService runService)
    {
        _buildService = buildService;
        _runService   = runService;

        BuildCommand       = new AsyncRelayCommand(BuildAsync, () => CanBuildOrRun);
        RunCommand         = new AsyncRelayCommand(RunAsync,   () => CanBuildOrRun);
        StopCommand        = new AsyncRelayCommand(StopAsync);
        CleanCommand       = new AsyncRelayCommand(CleanAsync, () => CanBuildOrRun);
        ClearOutputCommand = new RelayCommand(ClearOutput);

        // Forward build output to the Output panel
        _buildService.OutputReceived += AppendLine;

        // Use StatusChanged to detect when a build completes (Success or Failed)
        _buildService.StatusChanged += status =>
        {
            if (status == DroidIDE.Core.Enums.BuildStatus.Success)
                AppendLine("\n── Build succeeded ──");
            else if (status == DroidIDE.Core.Enums.BuildStatus.Failed)
                AppendLine("\n── Build FAILED ──");
        };

        // Forward run-process output to the Output panel
        _runService.OutputReceived += AppendLine;
        _runService.ErrorReceived  += line => AppendLine($"[stderr] {line}");
        _runService.Exited         += code =>
        {
            AppendLine($"\n── Process exited with code {code} ──");
            IsRunning = false;
            BuildStatus = "Ready";
        };
    }

    // ── Public API ────────────────────────────────────────────────

    /// <summary>
    /// Sets the active project/folder path so Build/Run know where to operate.
    /// </summary>
    public void SetProjectPath(string projectPath) => _currentProjectPath = projectPath;

    // ── Commands ──────────────────────────────────────────────────

    private async Task BuildAsync()
    {
        if (!EnsureProjectPath()) return;

        IsRunning = true;
        BuildStatus = "Building";
        BuildStarted?.Invoke();
        AppendLine($"── Building {Path.GetFileName(_currentProjectPath!)} ──");

        try
        {
            // Find the first .csproj under the open path
            var csproj = FindCsProj(_currentProjectPath!);
            if (csproj is null)
            {
                AppendLine("No .csproj found in the open folder.");
                BuildStatus = "Failed";
                return;
            }

            var result = await _buildService.RestoreAndBuildAsync(csproj);
            BuildStatus = result.Success ? "Succeeded" : "Failed";
        }
        catch (Exception ex)
        {
            AppendLine($"[ERROR] {ex.Message}");
            BuildStatus = "Failed";
        }
        finally
        {
            IsRunning = false;
        }
    }

    private async Task RunAsync()
    {
        if (!EnsureProjectPath()) return;

        // Build first, then run
        await BuildAsync();
        if (BuildStatus != "Succeeded") return;

        var csproj = FindCsProj(_currentProjectPath!);
        if (csproj is null) return;

        IsRunning = true;
        BuildStatus = "Running";
        BuildStarted?.Invoke();
        AppendLine($"\n── Running {Path.GetFileName(csproj)} ──");

        try
        {
            await _runService.StartAsync(csproj);
        }
        catch (Exception ex)
        {
            AppendLine($"[ERROR] {ex.Message}");
            IsRunning = false;
            BuildStatus = "Failed";
        }
    }

    private async Task StopAsync()
    {
        AppendLine("── Stopping process ──");
        await _runService.StopAsync();
        IsRunning = false;
        BuildStatus = "Ready";
    }

    private async Task CleanAsync()
    {
        if (!EnsureProjectPath()) return;

        IsRunning = true;
        BuildStatus = "Cleaning";
        BuildStarted?.Invoke();
        AppendLine($"── Cleaning ──");

        try
        {
            var csproj = FindCsProj(_currentProjectPath!);
            if (csproj is not null)
                await _buildService.CleanAsync(csproj);

            AppendLine("── Clean complete ──");
            BuildStatus = "Ready";
        }
        catch (Exception ex)
        {
            AppendLine($"[ERROR] {ex.Message}");
            BuildStatus = "Failed";
        }
        finally
        {
            IsRunning = false;
        }
    }

    private void ClearOutput()
    {
        BuildOutput.Clear();
        BuildStatus = "Ready";
    }

    // ── Helpers ───────────────────────────────────────────────────

    private void AppendLine(string line)
    {
        MainThread.BeginInvokeOnMainThread(() => BuildOutput.Add(line));
    }

    private bool EnsureProjectPath()
    {
        if (!string.IsNullOrEmpty(_currentProjectPath)) return true;
        AppendLine("No folder open. Open a project folder first.");
        return false;
    }

    private static string? FindCsProj(string rootPath)
    {
        try
        {
            return Directory.EnumerateFiles(rootPath, "*.csproj", SearchOption.AllDirectories)
                            .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
}
