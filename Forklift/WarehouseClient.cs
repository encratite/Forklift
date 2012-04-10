using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Forklift
{
	class WarehouseClient
	{
		Configuration Configuration;
		Thread ClientThread;
		bool Running;

		TcpClient Client;
		SslStream Stream;

		public WarehouseClient(Configuration configuration)
		{
			Configuration = configuration;
			Running = false;
		}

		public void Run()
		{
			ClientThread = new Thread(RunThread);
			ClientThread.Name = "Notification/RPC thread";
			ClientThread.Start();
			Running = true;
		}

		public void Terminate()
		{
			if (Running)
			{
				Running = false;
				ClientThread.Join();
				Stream.Close();
				Client.Close();
			}
		}

		void RunThread()
		{
			while (Running)
			{
				try
				{
					ProcessConnection();
				}
				catch (Exception exception)
				{
					Console.WriteLine("NRPC exception: {0}", exception);
				}
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

			NRPCProtocol protocolHandler = new NRPCProtocol(Stream);
			protocolHandler.Test();
		}
	}
}
