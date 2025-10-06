using Microsoft.Maui.Controls;
using ExcelClone.Views;

namespace ExcelClone;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(StartingPage), typeof(StartingPage));
		Routing.RegisterRoute(nameof(SpreadsheetPage), typeof(SpreadsheetPage));
	}
}
