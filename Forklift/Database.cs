using System.Collections.Generic;

namespace Forklift
{
	public class Database
	{
		public long NotificationCount { get; set; }

		public List<Notification> Notifications { get; set; }

		public Database()
		{
			NotificationCount = 0;
			Notifications = new List<Notification>();
		}
	}
}
