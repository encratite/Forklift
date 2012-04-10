using System.Collections.Generic;

namespace Forklift
{
	public class ServiceMessageNotification : BaseNotification
	{
		public readonly string Severity;
		public readonly string Message;

		public ServiceMessageNotification(Dictionary<string, object> input)
		{
			Severity = (string)input["severity"];
			Message = (string)input["message"];
		}
	}
}
