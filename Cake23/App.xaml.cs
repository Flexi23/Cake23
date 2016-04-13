using System;
using System.Windows;

namespace WpfApplication1
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		[STAThread]
		public static void Main()
		{
			var application = new App();
			application.InitializeComponent();
			application.Run();
		}
	}
}
