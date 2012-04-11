using System;

using Newtonsoft.Json;

namespace Forklift
{
	public class ReleaseDataNotification : Notification
	{
		[JsonProperty("site")]
		public string Site;

		[JsonProperty("siteId")]
		public int SiteId;

		[JsonProperty("name")]
		public string Name;


		[JsonConverter(typeof(UnixDateTimeConverter))]
		[JsonProperty("time")]
		public DateTime ReleaseTime;

		[JsonProperty("size")]
		public int Size;

		[JsonProperty("isManual")]
		public bool IsManual;
	}
}
