namespace Forklift
{
	public class QueuedNotification : ReleaseDataNotification
	{
		public override NotificationType GetNotificationType()
		{
			return NotificationType.Routine;
		}

		protected override string GetDescription()
		{
			return string.Format("Queued release: {0}", Name);
		}

		protected override string GetImageString()
		{
			return "ReleaseQueued";
		}
	}
}
