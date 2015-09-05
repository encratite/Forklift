using System;

using Newtonsoft.Json;

namespace Forklift
{
	public class NotificationData
	{
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime Time { get; set; }

		public string Type { get; set; }

		public object Content { get; set; }
	}
}
