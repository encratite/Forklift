using Newtonsoft.Json;

namespace Forklift
{
	public class DownloadDeletedNotification : Notification
	{
		[JsonProperty("release")]
		public string Release { get; set; }

		[JsonProperty("reason")]
		public string Reason { get; set; }

		public override NotificationType GetNotificationType()
		{
			return NotificationType.Error;
		}

		protected override string GetDescription()
		{
			return string.Format("Release \"{0}\" was deleted: {1}", Release, Reason);
		}

		protected override string GetImageString()
		{
			return "DownloadDeleted";
		}
	}
}
