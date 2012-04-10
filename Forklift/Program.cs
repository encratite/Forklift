using System;
using System.Threading;
using System.Xml;

using Nil;

namespace Forklift
{
	class Program
	{
		const string ConfigurationFile = "Configuration.xml";

		public static void Main(string[] arguments)
		{
			Configuration configuration;
			try
			{
				Serialiser<Configuration> serialiser = new Serialiser<Configuration>(ConfigurationFile);
				configuration = serialiser.Load();
			}
			catch (XmlException exception)
			{
				Console.WriteLine("Configuration error: {0}", exception.Message);
				return;
			}

			AutoResetEvent test = new AutoResetEvent(false);
			WarehouseClient client = new WarehouseClient(configuration);
			client.Run();
			test.WaitOne();
		}
	}
}
