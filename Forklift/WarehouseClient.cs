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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forklift
{
	public class WarehouseClient : INotificationHandler, IOutputHandler
	{
		Configuration Configuration;
		Thread ClientThread;
		bool Running;

		TcpClient Client;
		SslStream Stream;

		NRPCProtocol ProtocolHandler;

		Nil.Serialiser<Database> Serialiser;
		Database Database;

		Stopwatch NotificationRetrievalTimer;

		long ServerNotificationCount;

		MainWindow MainWindow;

		AutoResetEvent TerminationEvent;

		public WarehouseClient(Configuration configuration)
		{
			Configuration = configuration;
			ClientThread = null;
			Running = false;

			NotificationRetrievalTimer = new Stopwatch();

			TerminationEvent = new AutoResetEvent(false);

			LoadDatabase();

			MainWindow = new MainWindow(this);
		}

		public void Run()
		{
			if (Running)
				throw new Exception("The client is already running");
			ClientThread = new Thread(RunClient);
			ClientThread.Name = "Warehouse Client Thread";
			ClientThread.Start();
			Running = true;

			MainWindow.ShowDialog();
			MainWindow.NewNotification();
		}

		public void Terminate()
		{
			lock (Database)
			{
				if (Running)
				{
					Running = false;
					TerminationEvent.Set();
					Close();
					ClientThread.Join();
					ClientThread = null;
				}
			}
		}

		void LoadDatabase()
		{
			Serialiser = new Nil.Serialiser<Database>(Configuration.Database);
			Database = Serialiser.Load();
			foreach (var notification in Database.Notifications)
				notification.Initialise(false);
			Database.Notifications.Sort(CompareNotifications);
		}

		void SaveDatabase()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			lock (Database)
				Serialiser.Store(Database);
			stopwatch.Stop();
			//WriteLine("Saved {0} notifications in {1} ms", Database.Notifications.Count, stopwatch.ElapsedMilliseconds);
		}

		void NewNotification(Notification notification)
		{
			lock (Database)
			{
				Database.Notifications.Add(notification);
				Database.Notifications.Sort(CompareNotifications);
				Database.NotificationCount++;
			}
			SaveDatabase();
			PlayNotificationSound(notification);
			MainWindow.NewNotification();
		}

		void RunClient()
		{
			WriteLine("Number of notifications stored in the local database: {0}", Database.Notifications.Count);

			while (Running)
			{
				try
				{
					ProcessConnection();
				}
				catch (SocketException exception)
				{
					if (Running)
					{
						WriteLine("A connection error occurred: {0}", exception.Message);
						Reconnect();
					}
					else
						return;
				}
				catch (NRPCException exception)
				{
					if (Running)
					{
						WriteLine("An RPC exception occurred: {0}", exception.Message);
						Reconnect();
					}
					else
						return;
				}
			}
		}

		void Close()
		{
			if (Stream != null)
				Stream.Close();
			if (Client != null)
				Client.Close();
		}

		void Reconnect()
		{
			Close();
			WriteLine("Reconnecting in {0} ms", Configuration.ReconnectDelay);
			TerminationEvent.WaitOne(Configuration.ReconnectDelay);
		}

		void ProcessConnection()
		{
			WriteLine("Connecting to {0}:{1}", Configuration.Server.Host, Configuration.Server.Port);
			Client = new TcpClient(Configuration.Server.Host, Configuration.Server.Port);
			Stream = new SslStream(Client.GetStream(), false);
			X509Certificate certificate = new X509Certificate(Configuration.ClientCertificate);
			X509CertificateCollection collection = new X509CertificateCollection();
			collection.Add(certificate);
			Stream.AuthenticateAsClient(Configuration.Server.CommonName, collection, SslProtocols.Ssl3, false);

			WriteLine("Connected to the server");

			ProtocolHandler = new NRPCProtocol(Stream, this, this);
			ProtocolHandler.GetNotificationCount(GetNotificationCountCallback);
			while (Running)
				ProtocolHandler.ProcessUnit();
		}

		public void GetNotificationCountCallback(object[] arguments)
		{
			ServerNotificationCount = (long)arguments[0];
			long newNotificationCount = ServerNotificationCount - Database.NotificationCount;
			WriteLine("Number of notifications stored on the server: {0}", ServerNotificationCount);
			if (newNotificationCount > 0)
			{
				WriteLine("Number of new notifications available on the server: {0}", newNotificationCount);
				lock (Database)
					Database.NotificationCount = ServerNotificationCount;
				NotificationRetrievalTimer.Start();
				ProtocolHandler.GetNotifications(GetNotificationsCallback, 0, newNotificationCount);
			}
			else
				WriteLine("There are no new notifications available on the server");
		}

		public void GetNotificationsCallback(object[] arguments)
		{
			NotificationRetrievalTimer.Stop();
			JArray notificationObjects = (JArray)arguments[0];
			WriteLine("Downloaded {0} new notification(s) in {1} ms", notificationObjects.Count, NotificationRetrievalTimer.ElapsedMilliseconds);
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
			lock (Database)
			{
				Database.Notifications.AddRange(newNotifications);
				Database.Notifications.Sort(CompareNotifications);
				Database.NotificationCount = ServerNotificationCount;
			}
			SaveDatabase();
			if (errorOccurred)
				PlayErrorSound();
			else
				PlayNotificationSound();
			MainWindow.NewNotification();
		}

		int CompareNotifications(Notification x, Notification y)
		{
			return - x.Time.CompareTo(y.Time);
		}

		void PlaySound(UnmanagedMemoryStream resource)
		{
			SoundPlayer player = new SoundPlayer(resource);
			player.Play();
		}

		void PlayNotificationSound()
		{
			PlaySound(Properties.Resources.NotificationSound);
		}

		void PlayErrorSound()
		{
			PlaySound(Properties.Resources.ErrorSound);
		}

		void PlayNotificationSound(Notification notification)
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
			MainWindow.WriteLine(message, arguments);
		}

		public Database GetDatabase()
		{
			return Database;
		}
	}
}
