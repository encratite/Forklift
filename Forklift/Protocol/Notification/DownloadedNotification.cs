namespace Forklift
{
	public class DownloadedNotification : Notification
	{
		public readonly ReleaseData ReleaseData;

		public DownloadedNotification(NotificationData notificationData)
			: base(notificationData)
		{
			ReleaseData = new ReleaseData(notificationData.Content);
		}
	}
}
