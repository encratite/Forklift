using System;
using System.Xml;

namespace Forklift
{
	class Program
	{
		[STAThread]
		public static void Main(string[] arguments)
		{
			Configuration configuration;
			try
			{
				var serialiser = new Serializer<Configuration>("Configuration.xml");
				configuration = serialiser.Load();
			}
			catch (XmlException exception)
			{
				Console.WriteLine("Configuration error: {0}", exception.Message);
				return;
			}

			var client = new WarehouseClient(configuration);
			client.Run();
		}
	}
}
