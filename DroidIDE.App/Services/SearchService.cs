using System.Collections.Concurrent;

namespace DroidIDE.App.Services;

/// <summary>
/// Find-in-files search service that scans directory trees for text matches.
/// Supports case-sensitive/insensitive search and file extension filtering.
/// </summary>
public class SearchService
{
    /// <summary>Fired when search completes.</summary>
    public event Action<List<SearchResult>>? SearchCompleted;

    /// <summary>Fired during search with progress info.</summary>
    public event Action<int>? FilesScanned;

    /// <summary>
    /// Search for a text pattern in all files under a directory.
    /// </summary>
    public async Task<List<SearchResult>> SearchInFilesAsync(
        string rootDirectory,
        string searchText,
        bool caseSensitive = false,
        string[]? fileExtensions = null,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<SearchResult>();
        var filesScanned = 0;

        var files = GetSearchableFiles(rootDirectory, fileExtensions);

        await Parallel.ForEachAsync(files, cancellationToken, async (filePath, ct) =>
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(filePath, ct);
                var comparison = caseSensitive
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(searchText, comparison))
                    {
                        results.Add(new SearchResult
                        {
                            FilePath = filePath,
                            RelativePath = Path.GetRelativePath(rootDirectory, filePath),
                            LineNumber = i + 1,
                            LineContent = lines[i].Trim(),
                            MatchIndex = lines[i].IndexOf(searchText, comparison)
                        });
                    }
                }

                Interlocked.Increment(ref filesScanned);
                if (filesScanned % 50 == 0)
                    FilesScanned?.Invoke(filesScanned);
            }
            catch
            {
                // Skip files that can't be read (binary, locked, etc.)
            }
        });

        var resultList = results.OrderBy(r => r.FilePath).ThenBy(r => r.LineNumber).ToList();
        SearchCompleted?.Invoke(resultList);
        return resultList;
    }

    /// <summary>
    /// Get all searchable text files under a directory, respecting extension filters.
    /// Skips common binary/generated directories.
    /// </summary>
    private static IEnumerable<string> GetSearchableFiles(string rootDirectory, string[]? extensions)
    {
        var skipDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bin", "obj", ".git", ".vs", "node_modules", "packages", ".idea"
        };

        return Directory.EnumerateFiles(rootDirectory, "*.*", SearchOption.AllDirectories)
            .Where(f =>
            {
                // Skip files in excluded directories
                var relPath = Path.GetRelativePath(rootDirectory, f);
                var parts = relPath.Split(Path.DirectorySeparatorChar);
                if (parts.Any(p => skipDirs.Contains(p)))
                    return false;

                // Filter by extension if specified
                if (extensions is { Length: > 0 })
                {
                    var ext = Path.GetExtension(f).TrimStart('.');
                    return extensions.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase));
                }

                // Skip common binary extensions
                var fileExt = Path.GetExtension(f).ToLowerInvariant();
                return fileExt is not (".dll" or ".exe" or ".pdb" or ".png" or ".jpg"
                    or ".gif" or ".ico" or ".zip" or ".nupkg" or ".woff" or ".woff2");
            });
    }
}

/// <summary>
/// Represents a single search match within a file.
/// </summary>
public class SearchResult
{
    public string FilePath { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string LineContent { get; set; } = string.Empty;
    public int MatchIndex { get; set; }
}
