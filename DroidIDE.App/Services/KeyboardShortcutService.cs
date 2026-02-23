namespace DroidIDE.App.Services;

/// <summary>
/// Manages keyboard shortcut bindings and dispatches them to registered command handlers.
/// </summary>
public class KeyboardShortcutService
{
    private readonly Dictionary<string, string> _shortcuts = new(StringComparer.OrdinalIgnoreCase);
    private readonly CommandPaletteService _commandPalette;

    public KeyboardShortcutService(CommandPaletteService commandPalette)
    {
        _commandPalette = commandPalette;
        RegisterDefaults();
    }

    /// <summary>
    /// Register a keyboard shortcut mapping.
    /// </summary>
    public void RegisterShortcut(string keyCombo, string commandId)
    {
        _shortcuts[NormalizeKeyCombo(keyCombo)] = commandId;
    }

    /// <summary>
    /// Remove a keyboard shortcut.
    /// </summary>
    public void UnregisterShortcut(string keyCombo)
    {
        _shortcuts.Remove(NormalizeKeyCombo(keyCombo));
    }

    /// <summary>
    /// Get the command ID bound to a key combo, or null.
    /// </summary>
    public string? GetCommandForShortcut(string keyCombo)
    {
        _shortcuts.TryGetValue(NormalizeKeyCombo(keyCombo), out var commandId);
        return commandId;
    }

    /// <summary>
    /// Process a key event and execute the mapped command if any.
    /// Returns true if a shortcut was matched and executed.
    /// </summary>
    public bool ProcessKeyEvent(string keyCombo)
    {
        var normalized = NormalizeKeyCombo(keyCombo);
        if (_shortcuts.TryGetValue(normalized, out var commandId))
        {
            return _commandPalette.ExecuteCommand(commandId);
        }
        return false;
    }

    /// <summary>
    /// Get all registered shortcuts.
    /// </summary>
    public IReadOnlyDictionary<string, string> GetAllShortcuts() => _shortcuts;

    /// <summary>
    /// Normalize key combo to consistent format (Ctrl+Shift+S → ctrl+shift+s).
    /// </summary>
    private static string NormalizeKeyCombo(string keyCombo)
    {
        return keyCombo.Trim().ToLowerInvariant().Replace(" ", "");
    }

    /// <summary>
    /// Register default keyboard shortcuts.
    /// </summary>
    private void RegisterDefaults()
    {
        RegisterShortcut("ctrl+s", "editor.save");
        RegisterShortcut("ctrl+shift+s", "editor.saveAll");
        RegisterShortcut("ctrl+w", "editor.closeTab");
        RegisterShortcut("ctrl+shift+p", "palette.open");
        RegisterShortcut("ctrl+p", "file.quickOpen");
        RegisterShortcut("ctrl+shift+f", "search.findInFiles");
        RegisterShortcut("ctrl+b", "view.toggleExplorer");
        RegisterShortcut("ctrl+j", "view.toggleBottomPanel");
        RegisterShortcut("ctrl+shift+b", "build.build");
        RegisterShortcut("f5", "run.start");
        RegisterShortcut("shift+f5", "run.stop");
        RegisterShortcut("ctrl+shift+g", "git.open");
        RegisterShortcut("ctrl+n", "file.newFile");
        RegisterShortcut("ctrl+shift+n", "file.newProject");
        RegisterShortcut("ctrl+z", "edit.undo");
        RegisterShortcut("ctrl+y", "edit.redo");
    }
}
