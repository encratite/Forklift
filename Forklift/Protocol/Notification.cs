using System;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Forklift
{
	[XmlInclude(typeof(QueuedNotification))]
	[XmlInclude(typeof(DownloadedNotification))]
	[XmlInclude(typeof(DownloadError))]
	[XmlInclude(typeof(DownloadDeletedNotification))]
	[XmlInclude(typeof(ServiceMessage))]
	public abstract class Notification
	{
		[JsonIgnore]
		public DateTime Time;
	}
}
