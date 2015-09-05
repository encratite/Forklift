using Newtonsoft.Json;

namespace Forklift
{
	public class DownloadError : Notification
	{
		[JsonProperty("release")]
		public string Release { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		public override NotificationType GetNotificationType()
		{
			return NotificationType.Error;
		}

		protected override string GetDescription()
		{
			return string.Format("Download error in release \"{0}\": {1}", Release, Message);
		}

		protected override string GetImageString()
		{
			return "DownloadError";
		}
	}
}
