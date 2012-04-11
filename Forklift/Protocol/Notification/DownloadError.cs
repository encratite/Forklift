using Newtonsoft.Json;

namespace Forklift
{
	public class DownloadError : Notification
	{
		[JsonProperty("release")]
		public string Release;

		[JsonProperty("message")]
		public string Message;

		protected override string GetDescription()
		{
			return string.Format("Download error in release \"{0}\": {1}", Release, Message);
		}

		public override NotificationType GetNotificationType()
		{
			return NotificationType.Error;
		}
	}
}
