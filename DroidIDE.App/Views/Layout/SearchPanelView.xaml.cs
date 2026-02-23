using DroidIDE.App.ViewModels;

namespace DroidIDE.App.Views.Layout;

/// <summary>
/// Search panel view — find-in-files UI with results grouped by file.
/// </summary>
public partial class SearchPanelView : ContentView
{
    public SearchPanelView()
    {
        InitializeComponent();
    }

    private void OnCaseToggleTapped(object? sender, EventArgs e)
    {
        if (BindingContext is SearchViewModel vm)
        {
            vm.CaseSensitive = !vm.CaseSensitive;
            CaseToggle.BackgroundColor = vm.CaseSensitive
                ? Color.FromArgb("#0078D4")
                : Color.FromArgb("#3C3C3C");
        }
    }
}
