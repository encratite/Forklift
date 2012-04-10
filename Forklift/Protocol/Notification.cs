using System;

namespace Forklift
{
	public abstract class Notification
	{
		public readonly DateTime Time;

		public Notification(NotificationData notificationData)
		{
			Time = notificationData.Time;
		}
	}
}
