using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Forklift
{
	public class WarehouseClient : INotificationHandler, IOutputHandler
	{
		private Configuration _Configuration;
		private Thread _ClientThread;
		private bool _Running;

		private TcpClient _Client;
		private SslStream _Stream;

		private NRPCProtocol _ProtocolHandler;

		private Serializer<Database> _Serializer;
		private Database _Database;

		private Stopwatch _NotificationRetrievalTimer;

		private long _ServerNotificationCount;

		private MainWindow _MainWindow;

		private AutoResetEvent _TerminationEvent;

		public WarehouseClient(Configuration configuration)
		{
			_Configuration = configuration;
			_ClientThread = null;
			_Running = false;

			_NotificationRetrievalTimer = new Stopwatch();

			_TerminationEvent = new AutoResetEvent(false);

			LoadDatabase();

			_MainWindow = new MainWindow(this);
		}

		public void Run()
		{
			if (_Running)
				throw new Exception("The client is already running");
			_ClientThread = new Thread(RunClient);
			_ClientThread.Name = "Warehouse Client Thread";
			_ClientThread.Start();
			_Running = true;

			_MainWindow.ShowDialog();
			_MainWindow.NewNotification();
		}

		public void Terminate()
		{
			lock (_Database)
			{
				if (_Running)
				{
					_Running = false;
					_TerminationEvent.Set();
					Close();
					_ClientThread.Join();
					_ClientThread = null;
				}
			}
		}

		public void GetNotificationCountCallback(object[] arguments)
		{
			_ServerNotificationCount = (long)arguments[0];
			long newNotificationCount = _ServerNotificationCount - _Database.NotificationCount;
			WriteLine("Number of notifications stored on the server: {0}", _ServerNotificationCount);
			if (newNotificationCount > 0)
			{
				WriteLine("Number of new notifications available on the server: {0}", newNotificationCount);
				lock (_Database)
					_Database.NotificationCount = _ServerNotificationCount;
				_NotificationRetrievalTimer.Start();
				_ProtocolHandler.GetNotifications(GetNotificationsCallback, 0, newNotificationCount);
			}
			else
				WriteLine("There are no new notifications available on the server");
		}

		public void GetNotificationsCallback(object[] arguments)
		{
			_NotificationRetrievalTimer.Stop();
			JArray notificationObjects = (JArray)arguments[0];
			WriteLine("Downloaded {0} new notification(s) in {1} ms", notificationObjects.Count, _NotificationRetrievalTimer.ElapsedMilliseconds);
			List<Notification> newNotifications = new List<Notification>();
			bool errorOccurred = false;
			foreach (var notificationObject in notificationObjects)
			{
				try
				{
					Notification notification = NRPCProtocol.GetNotification(notificationObject);
					if (notification.GetNotificationType() != NotificationType.Routine)
						errorOccurred = true;
					newNotifications.Add(notification);
				}
				catch (NRPCException)
				{
					//This is probably an old test exception, ignore it
				}
			}
			//Make sure that the notifications are in the right order, commencing with the oldest one
			lock (_Database)
			{
				_Database.Notifications.AddRange(newNotifications);
				_Database.Notifications.Sort(CompareNotifications);
				_Database.NotificationCount = _ServerNotificationCount;
			}
			SaveDatabase();
			if (errorOccurred)
				PlayErrorSound();
			else
				PlayNotificationSound();
			_MainWindow.NewNotification();
		}

		private int CompareNotifications(Notification x, Notification y)
		{
			return - x.Time.CompareTo(y.Time);
		}

		private void PlaySound(UnmanagedMemoryStream resource)
		{
			SoundPlayer player = new SoundPlayer(resource);
			player.Play();
		}

		private void PlayNotificationSound()
		{
			PlaySound(Properties.Resources.NotificationSound);
		}

		private void PlayErrorSound()
		{
			PlaySound(Properties.Resources.ErrorSound);
		}

		private void PlayNotificationSound(Notification notification)
		{
			switch (notification.GetNotificationType())
			{
				case NotificationType.Routine:
					PlayNotificationSound();
					break;

				default:
					PlayErrorSound();
					break;
			}
		}

		public void HandleQueuedNotification(QueuedNotification notification)
		{
			NewNotification(notification);
		}

		public void HandleDownloadedNotification(DownloadedNotification notification)
		{
			NewNotification(notification);
		}

		public void HandleDownloadError(DownloadError notification)
		{
			NewNotification(notification);
		}

		public void HandleDownloadDeletedNotification(DownloadDeletedNotification notification)
		{
			NewNotification(notification);
		}

		public void HandleServiceMessage(ServiceMessage notification)
		{
			NewNotification(notification);
		}

		public void HandlePing()
		{
		}

		public void HandleError(string message)
		{
			WriteLine("Protocol error: {0}", message);
			PlayErrorSound();
		}

		public void WriteLine(string message, params object[] arguments)
		{
			_MainWindow.WriteLine(message, arguments);
		}

		public Database GetDatabase()
		{
			return _Database;
		}

		private void LoadDatabase()
		{
			_Serializer = new Serializer<Database>(_Configuration.Database);
			_Database = _Serializer.Load();
			foreach (var notification in _Database.Notifications)
				notification.Initialise(false);
			_Database.Notifications.Sort(CompareNotifications);
		}

		private void SaveDatabase()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			lock (_Database)
				_Serializer.Store(_Database);
			stopwatch.Stop();
		}

		private void NewNotification(Notification notification)
		{
			lock (_Database)
			{
				_Database.Notifications.Add(notification);
				_Database.Notifications.Sort(CompareNotifications);
				_Database.NotificationCount++;
			}
			SaveDatabase();
			PlayNotificationSound(notification);
			_MainWindow.NewNotification();
		}

		private void RunClient()
		{
			WriteLine("Number of notifications stored in the local database: {0}", _Database.Notifications.Count);

			while (_Running)
			{
				try
				{
					ProcessConnection();
				}
				catch (SocketException exception)
				{
					if (_Running)
					{
						WriteLine("A connection error occurred: {0}", exception.Message);
						Reconnect();
					}
					else
						return;
				}
				catch (NRPCException exception)
				{
					if (_Running)
					{
						WriteLine("An RPC exception occurred: {0}", exception.Message);
						Reconnect();
					}
					else
						return;
				}
			}
		}

		private void Close()
		{
			if (_Stream != null)
				_Stream.Close();
			if (_Client != null)
				_Client.Close();
		}

		private void Reconnect()
		{
			Close();
			WriteLine("Reconnecting in {0} ms", _Configuration.ReconnectDelay);
			_TerminationEvent.WaitOne(_Configuration.ReconnectDelay);
		}

		private void ProcessConnection()
		{
			WriteLine("Connecting to {0}:{1}", _Configuration.Server.Host, _Configuration.Server.Port);
			_Client = new TcpClient(_Configuration.Server.Host, _Configuration.Server.Port);
			_Stream = new SslStream(_Client.GetStream(), false);
			X509Certificate certificate = new X509Certificate(_Configuration.ClientCertificate);
			X509CertificateCollection collection = new X509CertificateCollection();
			collection.Add(certificate);
			_Stream.AuthenticateAsClient(_Configuration.Server.CommonName, collection, SslProtocols.Ssl3, false);

			WriteLine("Connected to the server");

			_ProtocolHandler = new NRPCProtocol(_Stream, this, this);
			_ProtocolHandler.GetNotificationCount(GetNotificationCountCallback);
			while (_Running)
				_ProtocolHandler.ProcessUnit();
		}
	}
}
