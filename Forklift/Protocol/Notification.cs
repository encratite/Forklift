using System;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Forklift
{
	[XmlInclude(typeof(QueuedNotification))]
	[XmlInclude(typeof(DownloadedNotification))]
	public abstract class Notification
	{
		[JsonIgnore]
		public DateTime Time;
	}
}
