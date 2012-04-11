using System;

using Newtonsoft.Json;

namespace Forklift
{
	public abstract class ReleaseDataNotification : Notification
	{
		[JsonProperty("site")]
		public string Site;

		[JsonProperty("siteId")]
		public long SiteId;

		[JsonProperty("name")]
		public string Name;


		[JsonConverter(typeof(UnixDateTimeConverter))]
		[JsonProperty("time")]
		public DateTime ReleaseTime;

		[JsonProperty("size")]
		public long Size;

		[JsonProperty("isManual")]
		public bool IsManual;
	}
}
