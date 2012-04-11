using Newtonsoft.Json;

namespace Forklift
{
	public class ServiceMessage : Notification
	{
		[JsonProperty("severity")]
		public string Severity;

		[JsonProperty("message")]
		public string Message;
	}
}
