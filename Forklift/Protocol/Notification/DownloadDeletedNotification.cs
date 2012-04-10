namespace Forklift
{
	public class DownloadDeletedNotification : Notification
	{
		public readonly string Release;
		public readonly string Reason;

		public DownloadDeletedNotification(NotificationData notificationData)
			: base(notificationData)
		{
			var content = notificationData.Content;
			Release = (string)content["release"];
			Reason = (string)content["reason"];
		}
	}
}
