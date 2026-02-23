using System.Text.Json;

namespace DroidIDE.Infrastructure.Security;

/// <summary>
/// Manages credential storage using a JSON file within the app's sandboxed data directory.
/// On Android, the app's internal storage is already sandboxed per-app by the OS,
/// providing isolation without requiring MAUI SecureStorage.
/// </summary>
public class CredentialStore
{
    private readonly string _storePath;
    private Dictionary<string, string> _credentials = new();
    private bool _loaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="CredentialStore"/> class.
    /// </summary>
    /// <param name="storagePath">
    /// Optional custom path for the credentials JSON file.
    /// Defaults to <c>{LocalApplicationData}/droidide_credentials.json</c>.
    /// </param>
    public CredentialStore(string? storagePath = null)
    {
        _storePath = storagePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "droidide_credentials.json");
    }

    /// <summary>
    /// Stores a credential key-value pair, persisting it to disk immediately.
    /// </summary>
    /// <param name="key">The credential identifier (e.g., "github_token").</param>
    /// <param name="value">The credential value to store.</param>
    public async Task StoreAsync(string key, string value)
    {
        await EnsureLoadedAsync();
        _credentials[key] = value;
        await SaveAsync();
    }

    /// <summary>
    /// Retrieves a stored credential by its key.
    /// </summary>
    /// <param name="key">The credential identifier to look up.</param>
    /// <returns>The credential value, or <c>null</c> if the key is not found.</returns>
    public async Task<string?> GetAsync(string key)
    {
        await EnsureLoadedAsync();
        return _credentials.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Removes a stored credential by its key and persists the change to disk.
    /// </summary>
    /// <param name="key">The credential identifier to remove.</param>
    public async Task RemoveAsync(string key)
    {
        await EnsureLoadedAsync();
        _credentials.Remove(key);
        await SaveAsync();
    }

    /// <summary>
    /// Clears all stored credentials and persists the empty store to disk.
    /// </summary>
    public async Task RemoveAllAsync()
    {
        _credentials.Clear();
        await SaveAsync();
    }

    /// <summary>
    /// Lazily loads credentials from the JSON file on first access.
    /// Subsequent calls are no-ops due to the <see cref="_loaded"/> flag.
    /// </summary>
    private async Task EnsureLoadedAsync()
    {
        if (_loaded) return;

        if (File.Exists(_storePath))
        {
            var json = await File.ReadAllTextAsync(_storePath);
            _credentials = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        }

        _loaded = true;
    }

    /// <summary>
    /// Serializes the in-memory credential dictionary to JSON and writes it to the store file.
    /// Creates the parent directory if it doesn't exist.
    /// </summary>
    private async Task SaveAsync()
    {
        var dir = Path.GetDirectoryName(_storePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_credentials);
        await File.WriteAllTextAsync(_storePath, json);
    }
}
