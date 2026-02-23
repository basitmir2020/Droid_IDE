using DroidIDE.App.Services;

namespace DroidIDE.App.Views.Layout;

/// <summary>
/// Command palette overlay — search and execute commands.
/// </summary>
public partial class CommandPaletteView : ContentView
{
    private CommandPaletteService? _paletteService;

    public CommandPaletteView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Inject the command palette service.
    /// </summary>
    public void SetService(CommandPaletteService service)
    {
        _paletteService = service;
        _paletteService.PaletteRequested += Show;
        RefreshCommands();
    }

    /// <summary>
    /// Show the command palette overlay.
    /// </summary>
    public void Show()
    {
        IsVisible = true;
        SearchEntry.Text = string.Empty;
        RefreshCommands();
        SearchEntry.Focus();
    }

    /// <summary>
    /// Hide the command palette overlay.
    /// </summary>
    public void Hide()
    {
        IsVisible = false;
        SearchEntry.Unfocus();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        RefreshCommands(e.NewTextValue);
    }

    private void OnCommandSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CommandEntry command)
        {
            _paletteService?.ExecuteCommand(command.Id);
            Hide();
        }
    }

    private void OnBackdropTapped(object? sender, EventArgs e)
    {
        Hide();
    }

    private void RefreshCommands(string? filter = null)
    {
        if (_paletteService is null) return;
        var commands = _paletteService.GetCommands(filter).ToList();
        CommandList.ItemsSource = commands;
    }
}
