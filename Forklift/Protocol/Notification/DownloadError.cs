using Newtonsoft.Json;

namespace Forklift
{
	public class DownloadError : Notification
	{
		[JsonProperty("release")]
		public string Release;

		[JsonProperty("message")]
		public string Message;
	}
}
