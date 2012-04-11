using Newtonsoft.Json;

namespace Forklift
{
	public class ServiceMessage : Notification
	{
		[JsonProperty("severity")]
		public string Severity;

		[JsonProperty("message")]
		public string Message;

		protected override string GetDescription()
		{
			return string.Format("Service message level \"{0}\": {1}", Severity, Message);
		}
	}
}
