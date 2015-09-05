using Newtonsoft.Json;

namespace Forklift
{
	public class ServiceMessage : Notification
	{
		[JsonProperty("severity")]
		public string Severity { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		public override NotificationType GetNotificationType()
		{
			if (Severity == "warning" || Severity == "error")
				return NotificationType.Error;
			else
				return NotificationType.Information;
		}

		protected override string GetDescription()
		{
			return string.Format("Service message level \"{0}\": {1}", Severity, Message);
		}

		protected override string GetImageString()
		{
			return "ServiceMessage";
		}
	}
}
