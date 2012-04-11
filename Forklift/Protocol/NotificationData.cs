using System;

using Newtonsoft.Json;

namespace Forklift
{
	public class NotificationData
	{
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime Time;
		public string Type;
		public object Content;
	}
}
