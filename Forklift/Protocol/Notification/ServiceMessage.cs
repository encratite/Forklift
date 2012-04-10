namespace Forklift
{
	public class ServiceMessage : Notification
	{
		public readonly string Severity;
		public readonly string Message;

		public ServiceMessage(NotificationData notificationData)
			: base(notificationData)
		{
			var content = notificationData.Content;
			Severity = (string)content["severity"];
			Message = (string)content["message"];
		}
	}
}
