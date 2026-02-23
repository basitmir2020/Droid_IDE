using DroidIDE.App.Views.Shell;

namespace DroidIDE.App;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainShell());
	}
}