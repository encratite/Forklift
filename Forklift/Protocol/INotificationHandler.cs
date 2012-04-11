namespace Forklift
{
	interface INotificationHandler
	{
		void HandleQueuedNotification(QueuedNotification notification);
		void HandleDownloadedNotification(DownloadedNotification notification);
		void HandleDownloadError(DownloadError notification);
		void HandleDownloadDeletedNotification(DownloadDeletedNotification notification);
		void HandleServiceMessage(ServiceMessage notification);

		void HandlePing();
		void HandleError(string message);
	}
}
