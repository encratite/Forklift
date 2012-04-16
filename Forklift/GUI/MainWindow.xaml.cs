using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Forklift
{
	public partial class MainWindow : Window
	{
		WarehouseClient WarehouseClient;

		bool IsFirstLine;

		public MainWindow(WarehouseClient warehouseClient)
		{
			WarehouseClient = warehouseClient;

			IsFirstLine = true;

			DataContext = new MainWindowDataContext(WarehouseClient.GetDatabase());

			InitializeComponent();

			int revision = Assembly.GetEntryAssembly().GetName().Version.Revision;
			Title = string.Format("Forklift r{0}", revision);
		}

		void WindowClosing(object sender, CancelEventArgs arguments)
		{
			WarehouseClient.Terminate();
		}

		public void WriteLine(string message, params object[] arguments)
		{
			message = string.Format(message, arguments);
			message = string.Format("{0} {1}", Nil.Time.Timestamp(), message);
			if (IsFirstLine)
				IsFirstLine = false;
			else
				message = "\n" + message;

			var action = (Action)delegate
			{
				lock (OutputTextBox)
				{
					OutputTextBox.AppendText(message);
					OutputTextBox.ScrollToEnd();
				}
			};

			OutputTextBox.Dispatcher.Invoke(action);
		}

		void ScrollToTop()
		{
			var action = (Action)delegate
			{
				var treeChild = (Decorator)VisualTreeHelper.GetChild(NotificationGrid, 0);
				if (treeChild != null)
				{
					var scrollViewer = (ScrollViewer)treeChild.Child;
					scrollViewer.ScrollToTop();
				}
			};

			NotificationGrid.Dispatcher.Invoke(action);
		}

		public void NewNotification()
		{
			ScrollToTop();
			NotificationGrid.Items.Refresh();
		}
	}
}
