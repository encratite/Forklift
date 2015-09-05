namespace Forklift
{
	public class Configuration
	{
		public ServerConfiguration Server { get; set; }

		public string Database { get; set; }

		public string ClientCertificate { get; set; }

		public int ReconnectDelay { get; set; }
	}
}
