using Newtonsoft.Json;

namespace Forklift
{
	public class DownloadDeletedNotification : Notification
	{
		[JsonProperty("release")]
		public string Release;

		[JsonProperty("reason")]
		public string Reason;
	}
}
