namespace Forklift
{
	public class QueuedNotification : ReleaseDataNotification
	{
		protected override string GetDescription()
		{
			return string.Format("Queued release: {0}", Name);
		}

		public override NotificationType GetNotificationType()
		{
			return NotificationType.Routine;
		}
	}
}
