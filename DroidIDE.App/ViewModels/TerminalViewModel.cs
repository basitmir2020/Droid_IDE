using System.Collections.ObjectModel;
using System.Windows.Input;
using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.App.ViewModels;

/// <summary>
/// ViewModel for the terminal panel — manages terminal sessions and I/O.
/// </summary>
public class TerminalViewModel : BaseViewModel
{
    private readonly ITerminalService _terminalService;

    public ObservableCollection<TerminalSession> Sessions { get; } = [];
    public ObservableCollection<string> OutputLines { get; } = [];

    private TerminalSession? _activeSession;
    public TerminalSession? ActiveSession
    {
        get => _activeSession;
        set
        {
            if (SetProperty(ref _activeSession, value))
                _ = RefreshOutputAsync();
        }
    }

    private string _commandInput = string.Empty;
    public string CommandInput
    {
        get => _commandInput;
        set => SetProperty(ref _commandInput, value);
    }

    public ICommand SendCommandCommand { get; }
    public ICommand NewSessionCommand { get; }
    public ICommand CloseSessionCommand { get; }
    public ICommand ClearOutputCommand { get; }

    public TerminalViewModel(ITerminalService terminalService)
    {
        _terminalService = terminalService;
        Title = "Terminal";

        SendCommandCommand = new AsyncRelayCommand(SendCommandAsync);
        NewSessionCommand = new AsyncRelayCommand(CreateSessionAsync);
        CloseSessionCommand = new AsyncRelayCommand(CloseActiveSessionAsync);
        ClearOutputCommand = new RelayCommand(ClearOutput);

        // Listen for real-time output
        _terminalService.OutputReceived += OnOutputReceived;
    }

    /// <summary>
    /// Creates a new terminal session in the given working directory.
    /// </summary>
    public async Task CreateSessionAsync(string workingDirectory)
    {
        var session = await _terminalService.CreateSessionAsync(workingDirectory);
        Sessions.Add(session);
        ActiveSession = session;
    }

    private async Task CreateSessionAsync()
    {
        await CreateSessionAsync(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
    }

    /// <summary>
    /// Sends the current command input to the active terminal session.
    /// </summary>
    private async Task SendCommandAsync()
    {
        if (ActiveSession is null || string.IsNullOrWhiteSpace(CommandInput))
            return;

        await _terminalService.SendCommandAsync(ActiveSession.Id, CommandInput);
        CommandInput = string.Empty;
    }

    /// <summary>
    /// Closes the currently active terminal session.
    /// </summary>
    private async Task CloseActiveSessionAsync()
    {
        if (ActiveSession is null) return;

        await _terminalService.CloseSessionAsync(ActiveSession.Id);
        Sessions.Remove(ActiveSession);
        ActiveSession = Sessions.LastOrDefault();
    }

    private void ClearOutput()
    {
        OutputLines.Clear();
        if (ActiveSession is not null)
            ActiveSession.OutputLines.Clear();
    }

    private async Task RefreshOutputAsync()
    {
        OutputLines.Clear();
        if (ActiveSession is null) return;

        var output = await _terminalService.GetOutputAsync(ActiveSession.Id);
        foreach (var line in output)
            OutputLines.Add(line);
    }

    private void OnOutputReceived(string sessionId, string line)
    {
        if (ActiveSession?.Id == sessionId)
        {
            // Must dispatch to UI thread
            MainThread.BeginInvokeOnMainThread(() => OutputLines.Add(line));
        }
    }
}
