using Cake23.Util;
using System;
using System.Windows;

namespace Cake23
{
	public partial class MainWindow : Window
	{
		Cake23Application cake23 = new Cake23Application();

		public MainWindow()
		{
			InitializeComponent();
			DataContext = cake23;
			cake23.Logger.Msg += cake23_Msg;
			cake23.Setup();
		}

		void cake23_Msg(string msg)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(new Msg(cake23_Msg), msg);
				return;
			}
			cake23.Log = string.Join(Environment.NewLine, cake23.Logger.History);
			Application.Current.Dispatcher.Invoke(() => log.ScrollToEnd());
		}
	}
}
