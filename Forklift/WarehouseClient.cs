using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forklift
{
	class WarehouseClient : INotificationHandler, IOutputHandler
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

		public WarehouseClient(Configuration configuration)
		{
			Configuration = configuration;
			ClientThread = null;
			Running = false;

			NotificationRetrievalTimer = new Stopwatch();

			LoadDatabase();
		}

		public void Run()
		{
			if (Running)
				throw new Exception("The client is already running");
			ClientThread = new Thread(RunClient);
			ClientThread.Name = "Warehouse Client Thread";
			ClientThread.Start();
			Running = true;
		}

		public void Terminate()
		{
			lock (Database)
			{
				if (Running)
				{
					Stream.Close();
					Client.Close();
					ClientThread.Join();
					Running = false;
					ClientThread = null;
				}
			}
		}

		void LoadDatabase()
		{
			Serialiser = new Nil.Serialiser<Database>(Configuration.Database);
			Database = Serialiser.Load();
			WriteLine("Number of notifications stored in the local database: {0}", Database.Notifications.Count);
		}

		void SaveDatabase()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			lock (Database)
			{
				Database.Notifications.Sort((x, y) => x.Time.CompareTo(y.Time));
				Serialiser.Store(Database);
			}
			stopwatch.Stop();
			WriteLine("Saved {0} notifications in {1} ms", Database.Notifications.Count, stopwatch.ElapsedMilliseconds);
		}

		void StoreNotification(Notification notification)
		{
			lock (Database)
			{
				Database.Notifications.Add(notification);
				Database.NotificationCount++;
			}
			SaveDatabase();
		}

		void RunClient()
		{
			while (Running)
			{
				ProcessConnection();
			}
		}

		void ProcessConnection()
		{
			Client = new TcpClient(Configuration.Server.Host, Configuration.Server.Port);
			Stream = new SslStream(Client.GetStream(), false);
			X509Certificate certificate = new X509Certificate(Configuration.ClientCertificate);
			X509CertificateCollection collection = new X509CertificateCollection();
			collection.Add(certificate);
			Stream.AuthenticateAsClient(Configuration.Server.CommonName, collection, SslProtocols.Ssl3, false);

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
				ProtocolHandler.GetNotifications(GetNotificationsCallback, 0, newNotificationCount - 1);
			}
			else
				WriteLine("There are no new notifications available on the server");
		}

		public void GetNotificationsCallback(object[] arguments)
		{
			NotificationRetrievalTimer.Stop();
			JArray notificationObjects = (JArray)arguments[0];
			WriteLine("Downloaded {0} new notification(s) in {1} ms", notificationObjects.Count, NotificationRetrievalTimer.ElapsedMilliseconds);
			foreach (var notificationObject in notificationObjects)
			{
				try
				{
					Notification notification = NRPCProtocol.GetNotification(notificationObject);
					Database.Notifications.Add(notification);
				}
				catch (NRPCException)
				{
					//This is probably an old test exception
				}
			}
			lock (Database)
				Database.NotificationCount = ServerNotificationCount;
			SaveDatabase();
		}

		public void HandleQueuedNotification(QueuedNotification notification)
		{
			WriteLine("[{0}] Queued: {1}", notification.Time, notification.Name);
			StoreNotification(notification);
		}

		public void HandleDownloadedNotification(DownloadedNotification notification)
		{
			WriteLine("[{0}] Downloaded: {1}", notification.Time, notification.Name);
			StoreNotification(notification);
		}

		public void HandleDownloadError(DownloadError notification)
		{
			WriteLine("[{0}] Download error for release \"{1}\": {2}", notification.Time, notification.Release, notification.Message);
			StoreNotification(notification);
		}

		public void HandleDownloadDeletedNotification(DownloadDeletedNotification notification)
		{
			WriteLine("[{0}] Removed release \"{1}\": {2}", notification.Time, notification.Release, notification.Reason);
			StoreNotification(notification);
		}

		public void HandleServiceMessage(ServiceMessage notification)
		{
			WriteLine("[{0}] Service message of level \"{1}\": {2}", notification.Time, notification.Severity, notification.Message);
			StoreNotification(notification);
		}

		public void HandlePing()
		{
		}

		public void HandleError(string message)
		{
			WriteLine("Protocol error: {0}", message);
		}

		public void WriteLine(string message, params object[] arguments)
		{
			Console.WriteLine(message, arguments);
		}
	}
}
