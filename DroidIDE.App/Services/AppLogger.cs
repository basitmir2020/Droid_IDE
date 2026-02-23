using System.Collections.Concurrent;

namespace DroidIDE.App.Services;

/// <summary>
/// File-based application logger with log rotation, severity levels,
/// and thread-safe writing. Replaces basic debug logging.
/// </summary>
public class AppLogger
{
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private readonly ConcurrentQueue<string> _buffer = new();
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private const long MaxLogFileSize = 5 * 1024 * 1024; // 5 MB
    private const int MaxRotatedFiles = 5;

    public AppLogger()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DroidIDE", "logs");

        Directory.CreateDirectory(_logDirectory);
        _logFilePath = Path.Combine(_logDirectory, "droidide.log");
    }

    // ── Public API ──

    public void LogInfo(string message) => Log("INFO", message);
    public void LogWarning(string message) => Log("WARN", message);
    public void LogError(string message, Exception? ex = null) => Log("ERROR", FormatError(message, ex));
    public void LogDebug(string message) =>
#if DEBUG
        Log("DEBUG", message);
#else
        _ = message; // No-op in release
#endif

    /// <summary>Flush any buffered log entries to disk.</summary>
    public async Task FlushAsync()
    {
        await WriteBufferAsync();
    }

    /// <summary>Get the path to the current log file for diagnostics.</summary>
    public string LogFilePath => _logFilePath;

    // ── Internal ──

    private void Log(string level, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var threadId = Environment.CurrentManagedThreadId;
        var entry = $"[{timestamp}] [{level,-5}] [T{threadId:D3}] {message}";

        _buffer.Enqueue(entry);

        // Auto-flush when buffer hits 20 entries
        if (_buffer.Count >= 20)
        {
            _ = WriteBufferAsync();
        }

        System.Diagnostics.Debug.WriteLine(entry);
    }

    private async Task WriteBufferAsync()
    {
        if (!await _writeLock.WaitAsync(100)) return; // Skip if locked

        try
        {
            await RotateIfNeededAsync();

            var lines = new List<string>();
            while (_buffer.TryDequeue(out var line))
            {
                lines.Add(line);
            }

            if (lines.Count > 0)
            {
                await File.AppendAllLinesAsync(_logFilePath, lines);
            }
        }
        catch
        {
            // Swallow — logging should never crash the app
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private async Task RotateIfNeededAsync()
    {
        try
        {
            if (!File.Exists(_logFilePath)) return;

            var info = new FileInfo(_logFilePath);
            if (info.Length < MaxLogFileSize) return;

            // Shift rotated files: .log.4 → .log.5, .log.3 → .log.4, etc.
            for (var i = MaxRotatedFiles; i >= 1; i--)
            {
                var source = i == 1 ? _logFilePath : $"{_logFilePath}.{i - 1}";
                var dest = $"{_logFilePath}.{i}";

                if (File.Exists(source))
                {
                    if (File.Exists(dest)) File.Delete(dest);
                    File.Move(source, dest);
                }
            }

            // Create fresh log file
            await File.WriteAllTextAsync(_logFilePath, string.Empty);
        }
        catch
        {
            // Swallow rotation errors
        }
    }

    private static string FormatError(string message, Exception? ex)
    {
        if (ex is null) return message;
        return $"{message} | {ex.GetType().Name}: {ex.Message}\n    StackTrace: {ex.StackTrace}";
    }
}
