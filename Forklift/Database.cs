using System.Collections.Generic;

namespace Forklift
{
	public class Database
	{
		public long NotificationCount;
		public List<Notification> Notifications;

		public Database()
		{
			NotificationCount = 0;
			Notifications = new List<Notification>();
		}
	}
}
