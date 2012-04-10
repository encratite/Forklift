using System.Collections.Generic;

namespace Forklift
{
	public class DownloadDeletedNotification : BaseNotification
	{
		public readonly string Release;
		public readonly string Reason;

		public DownloadDeletedNotification(Dictionary<string, object> input)
		{
			Release = (string)input["release"];
			Reason = (string)input["reason"];
		}
	}
}
