using System.Collections.Generic;

namespace Forklift
{
	public class QueuedNotification : BaseNotification
	{
		public readonly ReleaseData ReleaseData;

		public QueuedNotification(Dictionary<string, object> input)
		{
			ReleaseData = new ReleaseData(input);
		}
	}
}
