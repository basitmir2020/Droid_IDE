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
            if (SetProperty(ref _selectedItem, value) && value is not null)
            {
                if (value.IsDirectory)
                {
                    _ = OpenFolderAsync(value.FullPath);
                }
                else
                {
                    _ = OpenFileAsync(value);
                }
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
    public ICommand CreateFileCommand { get; }
    public ICommand CreateFolderCommand { get; }
    public ICommand RenameCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand NavigateUpCommand { get; }

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
        CreateFileCommand = new AsyncRelayCommand<FileItem>(CreateFileAsync);
        CreateFolderCommand = new AsyncRelayCommand<FileItem>(CreateFolderAsync);
        RenameCommand = new AsyncRelayCommand<FileItem>(RenameAsync);
        DeleteCommand = new AsyncRelayCommand<FileItem>(DeleteAsync);
        NavigateUpCommand = new AsyncRelayCommand(NavigateUpAsync);
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

    private async Task NavigateUpAsync()
    {
        if (string.IsNullOrEmpty(CurrentPath)) return;

        var parent = Path.GetDirectoryName(CurrentPath);
        if (!string.IsNullOrEmpty(parent))
        {
            await OpenFolderAsync(parent);
        }
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

    private async Task CreateFileAsync(FileItem? parent)
    {
        var page = Application.Current?.Windows[0].Page;
        if (page == null) return;

        string? dir = parent?.IsDirectory == true ? parent.FullPath : CurrentPath;
        if (string.IsNullOrEmpty(dir)) return;

        string name = await page.DisplayPromptAsync("New File", "Enter file name:");
        if (string.IsNullOrEmpty(name)) return;

        try
        {
            var path = Path.Combine(dir, name);
            await _fileSystemService.CreateFileAsync(path);
            await LoadFilesAsync();
        }
        catch (Exception ex)
        {
            await page.DisplayAlertAsync("Error", $"Could not create file: {ex.Message}", "OK");
        }
    }

    private async Task CreateFolderAsync(FileItem? parent)
    {
        var page = Application.Current?.Windows[0].Page;
        if (page == null) return;

        string? dir = parent?.IsDirectory == true ? parent.FullPath : CurrentPath;
        if (string.IsNullOrEmpty(dir)) return;

        string name = await page.DisplayPromptAsync("New Folder", "Enter folder name:");
        if (string.IsNullOrEmpty(name)) return;

        try
        {
            var path = Path.Combine(dir, name);
            _fileSystemService.CreateDirectory(path);
            await LoadFilesAsync();
        }
        catch (Exception ex)
        {
            await page.DisplayAlertAsync("Error", $"Could not create folder: {ex.Message}", "OK");
        }
    }

    private async Task RenameAsync(FileItem item)
    {
        var page = Application.Current?.Windows[0].Page;
        if (page == null) return;

        string newName = await page.DisplayPromptAsync("Rename", "Enter new name:", initialValue: item.Name);
        if (string.IsNullOrEmpty(newName) || newName == item.Name) return;

        try
        {
            var parent = Path.GetDirectoryName(item.FullPath) ?? "";
            var newPath = Path.Combine(parent, newName);
            await _fileSystemService.RenameAsync(item.FullPath, newPath);
            await LoadFilesAsync();
        }
        catch (Exception ex)
        {
            await page.DisplayAlertAsync("Error", $"Could not rename: {ex.Message}", "OK");
        }
    }

    private async Task DeleteAsync(FileItem item)
    {
        var page = Application.Current?.Windows[0].Page;
        if (page == null) return;

        bool confirm = await page.DisplayAlertAsync("Delete", $"Are you sure you want to delete '{item.Name}'?", "Delete", "Cancel");
        if (!confirm) return;

        try
        {
            await _fileSystemService.DeleteAsync(item.FullPath);
            await LoadFilesAsync();
        }
        catch (Exception ex)
        {
            await page.DisplayAlertAsync("Error", $"Could not delete: {ex.Message}", "OK");
        }
    }
}
