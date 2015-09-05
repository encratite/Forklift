using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

		public void NewNotification()
		{
			var action = (Action)delegate
			{
				var treeChild = (Decorator)VisualTreeHelper.GetChild(NotificationGrid, 0);
				if (treeChild != null)
				{
					var scrollViewer = (ScrollViewer)treeChild.Child;
					scrollViewer.ScrollToTop();
				}

				NotificationGrid.Items.Refresh();
			};

			NotificationGrid.Dispatcher.Invoke(action);
		}
	}
}
