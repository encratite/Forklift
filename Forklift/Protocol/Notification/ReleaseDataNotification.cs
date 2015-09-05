using System;

using Newtonsoft.Json;

namespace Forklift
{
	public abstract class ReleaseDataNotification : Notification
	{
		[JsonProperty("site")]
		public string Site { get; set; }

		[JsonProperty("siteId")]
		public long SiteId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonConverter(typeof(UnixDateTimeConverter))]
		[JsonProperty("time")]
		public DateTime ReleaseTime { get; set; }

		[JsonProperty("size")]
		public long Size { get; set; }

		[JsonProperty("isManual")]
		public bool IsManual { get; set; }
	}
}
