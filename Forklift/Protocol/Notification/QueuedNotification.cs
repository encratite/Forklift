namespace Forklift
{
	public class QueuedNotification : Notification
	{
		public readonly ReleaseData ReleaseData;

		public QueuedNotification(NotificationData notificationData)
			: base(notificationData)
		{
			ReleaseData = new ReleaseData(notificationData.Content);
		}
	}
}
