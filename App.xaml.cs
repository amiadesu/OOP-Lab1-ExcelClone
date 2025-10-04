using Microsoft.Maui.Controls;
namespace ExcelClone;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
