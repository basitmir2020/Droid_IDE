using System.Collections.ObjectModel;
using System.Windows.Input;
using DroidIDE.Core.Interfaces;
using DroidIDE.Core.Models;

namespace DroidIDE.App.ViewModels;

/// <summary>
/// ViewModel for the file/folder explorer sidebar panel.
/// </summary>
public class ExplorerViewModel : BaseViewModel
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IEditorService _editorService;

    public ObservableCollection<FileItem> RootItems { get; } = [];

    private FileItem? _selectedItem;
    public FileItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value) && value is not null && !value.IsDirectory)
            {
                _ = OpenFileAsync(value);
            }
        }
    }

    private string _currentPath = string.Empty;
    public string CurrentPath
    {
        get => _currentPath;
        set => SetProperty(ref _currentPath, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand OpenFolderCommand { get; }

    /// <summary>
    /// Event raised when a file should be opened in the editor.
    /// </summary>
    public event Action<EditorTab>? FileOpenRequested;

    public ExplorerViewModel(IFileSystemService fileSystemService, IEditorService editorService)
    {
        _fileSystemService = fileSystemService;
        _editorService = editorService;
        Title = "Explorer";

        RefreshCommand = new AsyncRelayCommand(LoadFilesAsync);
        OpenFolderCommand = new AsyncRelayCommand(PickAndOpenFolderAsync);
    }

    /// <summary>
    /// Loads the file tree from the current path.
    /// </summary>
    public async Task LoadFilesAsync()
    {
        if (string.IsNullOrEmpty(CurrentPath) || !_fileSystemService.DirectoryExists(CurrentPath))
            return;

        IsBusy = true;
        try
        {
            RootItems.Clear();
            var tree = await _fileSystemService.GetFileTreeAsync(CurrentPath);
            foreach (var child in tree.Children)
            {
                RootItems.Add(child);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Sets the current path and loads the file tree.
    /// </summary>
    public async Task OpenFolderAsync(string folderPath)
    {
        CurrentPath = folderPath;
        await LoadFilesAsync();
    }

    /// <summary>
    /// Opens a file picker and uses the selected file's parent directory as the project folder.
    /// This approach works across all platforms without requiring CommunityToolkit.Maui.
    /// </summary>
    private async Task PickAndOpenFolderAsync()
    {
        try
        {
            var fileResult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select any file inside the folder you want to open"
            });

            if (fileResult != null)
            {
                var directoryPath = Path.GetDirectoryName(fileResult.FullPath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    await OpenFolderAsync(directoryPath);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FilePicker failed: {ex.Message}");
        }
    }

    private async Task OpenFileAsync(FileItem fileItem)
    {
        try
        {
            var tab = await _editorService.OpenFileAsync(fileItem.FullPath);
            FileOpenRequested?.Invoke(tab);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to open file: {ex.Message}");
        }
    }
}
