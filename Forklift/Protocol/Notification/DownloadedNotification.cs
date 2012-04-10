using System.Collections.Generic;

namespace Forklift
{
	public class DownloadedNotification : BaseNotification
	{
		public readonly ReleaseData ReleaseData;

		public DownloadedNotification(Dictionary<string, object> input)
		{
			ReleaseData = new ReleaseData(input);
		}
	}
}
