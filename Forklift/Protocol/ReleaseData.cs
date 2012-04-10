using System;
using System.Collections.Generic;

namespace Forklift
{
	public class ReleaseData
	{
		public readonly string Site;
		public readonly int SiteId;
		public readonly string Name;
		public readonly DateTime Time;
		public readonly int Size;
		public readonly bool IsManual;

		public ReleaseData(Dictionary<string, object> input)
		{
			Site = (string)input["site"];
			SiteId = (int)input["siteId"];
			Name = (string)input["name"];
			Time = Nil.Time.FromUnixTime((int)input["time"]);
			Size = (int)input["size"];
			IsManual = (bool)input["isManual"];
		}
	}
}
