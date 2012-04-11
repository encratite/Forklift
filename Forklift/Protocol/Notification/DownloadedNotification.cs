namespace Forklift
{
	public class DownloadedNotification : ReleaseDataNotification
	{
		protected override string GetDescription()
		{
			return string.Format("Download done: {0}", Name);
		}

		protected override string GetImageString()
		{
			return "ReleaseDownloaded";
		}

		public override NotificationType GetNotificationType()
		{
			return NotificationType.Routine;
		}
	}
}
