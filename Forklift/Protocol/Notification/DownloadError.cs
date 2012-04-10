using System.Collections.Generic;

namespace Forklift
{
	public class DownloadErrorNotification : BaseNotification
	{
		public readonly string Release;
		public readonly string Message;

		public DownloadErrorNotification(Dictionary<string, object> input)
		{
			Release = (string)input["release"];
			Message = (string)input["message"];
		}
	}
}
