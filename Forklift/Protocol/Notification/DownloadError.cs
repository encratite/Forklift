namespace Forklift
{
	public class DownloadError : Notification
	{
		public readonly string Release;
		public readonly string Message;

		public DownloadError(NotificationData notificationData)
			: base(notificationData)
		{
			var content = notificationData.Content;
			Release = (string)content["release"];
			Message = (string)content["message"];
		}
	}
}
