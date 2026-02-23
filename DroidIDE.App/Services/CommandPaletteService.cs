using System.Windows.Input;

namespace DroidIDE.App.Services;

/// <summary>
/// Registry and execution engine for the command palette.
/// Commands are registered by name and can be filtered/executed from a palette UI.
/// </summary>
public class CommandPaletteService
{
    private readonly Dictionary<string, CommandEntry> _commands = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Fired when the command palette should open.</summary>
    public event Action? PaletteRequested;

    /// <summary>Fired when a command is executed.</summary>
    public event Action<string>? CommandExecuted;

    /// <summary>
    /// Register a command with the palette.
    /// </summary>
    public void RegisterCommand(string id, string displayName, Action execute, string category = "General",
        string? shortcut = null)
    {
        _commands[id] = new CommandEntry
        {
            Id = id,
            DisplayName = displayName,
            Execute = execute,
            Category = category,
            Shortcut = shortcut
        };
    }

    /// <summary>
    /// Remove a registered command.
    /// </summary>
    public void UnregisterCommand(string id)
    {
        _commands.Remove(id);
    }

    /// <summary>
    /// Get all registered commands, optionally filtered by search query.
    /// </summary>
    public IEnumerable<CommandEntry> GetCommands(string? searchQuery = null)
    {
        var commands = _commands.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            commands = commands.Where(c =>
                c.DisplayName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                c.Category.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
        }

        return commands.OrderBy(c => c.Category).ThenBy(c => c.DisplayName);
    }

    /// <summary>
    /// Execute a command by ID.
    /// </summary>
    public bool ExecuteCommand(string id)
    {
        if (_commands.TryGetValue(id, out var entry))
        {
            entry.Execute();
            CommandExecuted?.Invoke(id);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Request the UI to open the command palette.
    /// </summary>
    public void ShowPalette() => PaletteRequested?.Invoke();
}

/// <summary>
/// Represents a registered command in the palette.
/// </summary>
public class CommandEntry
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Action Execute { get; set; } = () => { };
    public string Category { get; set; } = "General";
    public string? Shortcut { get; set; }
}
