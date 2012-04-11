using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Forklift
{
	class WarehouseClient : INotificationHandler
	{
		Configuration Configuration;
		Thread ClientThread;
		bool Running;

		TcpClient Client;
		SslStream Stream;

		public WarehouseClient(Configuration configuration)
		{
			Configuration = configuration;
			ClientThread = null;
			Running = false;
		}

		public void Run()
		{
			ClientThread = new Thread(RunClient);
			ClientThread.Name = "Warehouse Client Thread";
			ClientThread.Start();
			Running = true;
		}

		public void Terminate()
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

		void RunClient()
		{
			while (Running)
			{
				/*
				try
				{
					ProcessConnection();
				}
				catch (Exception exception)
				{
					Console.WriteLine("NRPC exception: {0}", exception);
				}
				*/
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

			NRPCProtocol protocolHandler = new NRPCProtocol(Stream, this);
			protocolHandler.GetNotificationCount(GetNotificationCountCallback);
			while (Running)
				protocolHandler.ProcessUnit();
		}

		public void GetNotificationCountCallback(object[] arguments)
		{
			long count = (long)arguments[0];
			Console.WriteLine("Count: {0}", count);
		}

		public void HandleQueuedNotification(QueuedNotification notification)
		{
		}

		public void HandleDownloadedNotification(DownloadedNotification notification)
		{
		}

		public void HandleDownloadError(DownloadError notification)
		{
		}

		public void HandleDownloadDeletedNotification(DownloadDeletedNotification notification)
		{
		}

		public void HandleServiceMessage(ServiceMessage notification)
		{
		}

		public void HandlePing()
		{
		}

		public void HandleError(string message)
		{
			Console.WriteLine("Protocol error: {0}", message);
		}
	}
}
