using System;
using System.Collections.Generic;

namespace Forklift
{
	public class NotificationData
	{
		public readonly DateTime Time;
		public readonly string Type;
		public readonly Dictionary<string, object> Content;

		public NotificationData(Dictionary<string, object> input)
		{
			Time = Nil.Time.FromUnixTime((int)input["time"]);
			Type = (string)input["type"];
			Content = (Dictionary<string, object>)input["content"];
		}
	}
}
