using Microsoft.Maui;
using Microsoft.Maui.Controls;
namespace ExcelClone;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState = null)
	{
		var startingPage = new AppShell();

		return new Window(startingPage);
    }
}
