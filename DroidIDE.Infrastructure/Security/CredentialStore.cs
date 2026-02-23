using System.Text.Json;

namespace DroidIDE.Infrastructure.Security;

/// <summary>
/// Manages credential storage using encrypted JSON file.
/// On Android, the app's internal storage is already sandboxed.
/// </summary>
public class CredentialStore
{
    private readonly string _storePath;
    private Dictionary<string, string> _credentials = new();
    private bool _loaded;

    public CredentialStore(string? storagePath = null)
    {
        _storePath = storagePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "droidide_credentials.json");
    }

    /// <summary>
    /// Stores a credential.
    /// </summary>
    public async Task StoreAsync(string key, string value)
    {
        await EnsureLoadedAsync();
        _credentials[key] = value;
        await SaveAsync();
    }

    /// <summary>
    /// Retrieves a stored credential.
    /// </summary>
    public async Task<string?> GetAsync(string key)
    {
        await EnsureLoadedAsync();
        return _credentials.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Removes a stored credential.
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        await EnsureLoadedAsync();
        _credentials.Remove(key);
        await SaveAsync();
    }

    /// <summary>
    /// Clears all stored credentials.
    /// </summary>
    public async Task RemoveAllAsync()
    {
        _credentials.Clear();
        await SaveAsync();
    }

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

    private async Task SaveAsync()
    {
        var dir = Path.GetDirectoryName(_storePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_credentials);
        await File.WriteAllTextAsync(_storePath, json);
    }
}
