using Newtonsoft.Json;

namespace Forklift
{
	public class DownloadDeletedNotification : Notification
	{
		[JsonProperty("release")]
		public string Release;

		[JsonProperty("reason")]
		public string Reason;

		protected override string GetDescription()
		{
			return string.Format("Release \"{0}\" was deleted: {1}", Release, Reason);
		}

		protected override string GetImageString()
		{
			return "DownloadDeleted";
		}

		public override NotificationType GetNotificationType()
		{
			return NotificationType.Error;
		}
	}
}
