using System.Diagnostics;
using DroidIDE.Core.Enums;
using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;
using DroidIDE.Terminal.OutputParser;

namespace DroidIDE.Runtime.DotnetManager;

/// <summary>
/// Implements <see cref="IDotnetCli"/> by wrapping all dotnet CLI operations.
/// Provides typed access to build, run, test, clean, restore, and SDK version commands.
/// Uses <see cref="IProcessManager"/> for process execution and <see cref="OutputLineParser"/>
/// for extracting structured diagnostics from build output.
/// </summary>
public class DotnetCliService : IDotnetCli
{
    private readonly IProcessManager _processManager;

    /// <summary>
    /// Initializes a new instance of <see cref="DotnetCliService"/>.
    /// </summary>
    /// <param name="processManager">The process manager used for executing dotnet CLI commands.</param>
    public DotnetCliService(IProcessManager processManager)
    {
        _processManager = processManager;
    }

    /// <inheritdoc/>
    public event Action<string>? BuildOutputReceived;

    /// <inheritdoc/>
    /// <remarks>
    /// Starts a streaming <c>dotnet build --no-restore</c> process, captures each output line,
    /// parses MSBuild diagnostics via <see cref="OutputLineParser.TryParseDiagnostic"/>,
    /// and waits for the process to exit before returning a structured <see cref="BuildResult"/>.
    /// </remarks>
    public async Task<BuildResult> BuildAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var diagnostics = new List<DiagnosticItem>();
        var outputLines = new List<string>();

        var dotnetBinary = ResolveDotnetBinary();
        if (!File.Exists(dotnetBinary) && dotnetBinary != "dotnet")
        {
            var errorLines = new List<string>
            {
                $"[ERROR] .NET SDK not found at: {dotnetBinary}",
                "",
                "💡 HOW TO GET .NET SDK ON ANDROID:",
                "DroidIDE requires the .NET SDK via Termux User Repository (TUR).",
                "1. Install Termux from F-Droid or GitHub.",
                "2. Run these commands in Termux:",
                "   pkg update && pkg upgrade",
                "   pkg install tur-repo",
                "   pkg install dotnet-sdk",
                "3. Restart DroidIDE.",
                "",
                "Alternative: Use 'proot-distro' to install Ubuntu and then .NET inside it."
            };
            outputLines.AddRange(errorLines);
            foreach (var line in errorLines) BuildOutputReceived?.Invoke(line);
            return new BuildResult { Success = false, Status = BuildStatus.Failed, OutputLines = outputLines };
        }

        var workingDir = Path.GetDirectoryName(projectPath) ?? ".";
        IRunningProcess process;
        try
        {
            process = await _processManager.StartAsync(dotnetBinary, $"build \"{projectPath}\" --no-restore",
                workingDir, cancellationToken);
        }
        catch (Exception ex)
        {
            var errorLines = new List<string>
            {
                $"[ERROR] Failed to start 'dotnet' process: {ex.Message}",
                "",
                "💡 DroidIDE could not find or start the .NET SDK.",
                "To fix this, please follow the installation guide below:",
                "",
                "1. Install Termux from F-Droid or GitHub.",
                "2. Run these commands in Termux:",
                "   pkg update && pkg upgrade",
                "   pkg install tur-repo",
                "   pkg install dotnet-sdk",
                "3. Restart DroidIDE.",
                "",
                "Current attempted binary: " + dotnetBinary
            };
            outputLines.AddRange(errorLines);
            foreach (var line in errorLines) BuildOutputReceived?.Invoke(line);
            return new BuildResult { Success = false, Status = BuildStatus.Failed, OutputLines = outputLines };
        }

        var tcs = new TaskCompletionSource<int>();

        process.OutputReceived += line =>
        {
            outputLines.Add(line);
            BuildOutputReceived?.Invoke(line);

            // Try to parse MSBuild diagnostics from each output line
            var diagnostic = OutputLineParser.TryParseDiagnostic(line);
            if (diagnostic is not null)
            {
                diagnostics.Add(diagnostic);
            }
        };

        process.ErrorReceived += line =>
        {
            outputLines.Add(line);
            BuildOutputReceived?.Invoke(line);
        };

        process.Exited += exitCode => tcs.TrySetResult(exitCode);

        var code = await tcs.Task;
        stopwatch.Stop();

        return new BuildResult
        {
            Success = code == 0,
            Status = code == 0 ? BuildStatus.Success : BuildStatus.Failed,
            Diagnostics = diagnostics,
            OutputLines = outputLines,
            Duration = stopwatch.Elapsed
        };
    }

    /// <inheritdoc/>
    public async Task<IRunningProcess> RunAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var workingDir = Path.GetDirectoryName(projectPath) ?? ".";
        return await _processManager.StartAsync(ResolveDotnetBinary(), $"run --project \"{projectPath}\"",
            workingDir, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProcessResult> NewAsync(string template, string outputDirectory, string name,
        CancellationToken cancellationToken = default)
    {
        return await _processManager.RunAsync(ResolveDotnetBinary(), $"new {template} -n \"{name}\" -o \"{outputDirectory}\"",
            outputDirectory, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProcessResult> RestoreAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var workingDir = Path.GetDirectoryName(projectPath) ?? ".";
        return await _processManager.RunAsync(ResolveDotnetBinary(), $"restore \"{projectPath}\"",
            workingDir, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProcessResult> TestAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var workingDir = Path.GetDirectoryName(projectPath) ?? ".";
        return await _processManager.RunAsync(ResolveDotnetBinary(), $"test \"{projectPath}\"",
            workingDir, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProcessResult> CleanAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var workingDir = Path.GetDirectoryName(projectPath) ?? ".";
        return await _processManager.RunAsync(ResolveDotnetBinary(), $"clean \"{projectPath}\"",
            workingDir, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string> GetSdkVersionAsync()
    {
        var result = await _processManager.RunAsync(ResolveDotnetBinary(), "--version", ".");
        return result.StandardOutput.Trim();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Runs <c>dotnet --version</c> and considers the SDK available if the command succeeds
    /// and produces non-empty output. Returns <c>false</c> on any exception (e.g., dotnet not found).
    /// </remarks>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var result = await _processManager.RunAsync(ResolveDotnetBinary(), "--version", ".");
            return result.Success && !string.IsNullOrWhiteSpace(result.StandardOutput);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Resolves the absolute path to the dotnet CLI binary.
    /// Checks (in order):
    /// 1. DOTNET_ROOT environment variable  →  $DOTNET_ROOT/dotnet
    /// 2. Common Termux paths on Android
    /// 3. Falls back to bare "dotnet" (works on Windows/macOS where it's in PATH)
    /// </summary>
    private static string ResolveDotnetBinary()
    {
        // 1. DOTNET_ROOT env var (set by LinuxEnvironmentManager.Configure())
        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrEmpty(dotnetRoot))
        {
            var candidate = Path.Combine(dotnetRoot, "dotnet");
            if (File.Exists(candidate)) return candidate;
        }

        // 2. Common Termux / proot-distro Android paths
        var termuxCandidates = new[]
        {
            "/data/data/com.termux/files/usr/bin/dotnet",
            "/data/data/com.termux/files/home/.dotnet/dotnet",
            "/data/user/0/com.termux/files/usr/bin/dotnet",
            // proot-distro ubuntu/debian/distro paths
            "/data/data/com.termux/files/usr/var/lib/proot-distro/installed-rootfs/ubuntu/usr/lib/dotnet/dotnet",
            "/data/data/com.termux/files/usr/var/lib/proot-distro/installed-rootfs/ubuntu/usr/bin/dotnet",
            "/data/data/com.termux/files/usr/var/lib/proot-distro/installed-rootfs/debian/usr/lib/dotnet/dotnet",
            "/data/data/com.termux/files/usr/var/lib/proot-distro/installed-rootfs/debian/usr/bin/dotnet",
            "/data/user/0/com.termux/files/usr/var/lib/proot-distro/installed-rootfs/ubuntu/usr/bin/dotnet",
            "/usr/bin/dotnet",
            "/usr/lib/dotnet/dotnet",
            "/usr/local/bin/dotnet",
            "/usr/lib/dotnet/dotnet"
        };

        foreach (var path in termuxCandidates)
        {
            try
            {
                if (File.Exists(path)) return path;
            }
            catch (UnauthorizedAccessException)
            {
                // Permission Denied for this path
                System.Diagnostics.Debug.WriteLine($"[DotnetCli] Permission Denied for path: {path}");
            }
        }

        // 3. Bare name — works on Windows/macOS/Linux where dotnet is in PATH
        return "dotnet";
    }
}

