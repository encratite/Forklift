using System;
using System.Xml.Serialization;
using System.ComponentModel;

using Newtonsoft.Json;

using Nil;

namespace Forklift
{
	[XmlInclude(typeof(QueuedNotification))]
	[XmlInclude(typeof(DownloadedNotification))]
	[XmlInclude(typeof(DownloadError))]
	[XmlInclude(typeof(DownloadDeletedNotification))]
	[XmlInclude(typeof(ServiceMessage))]
	public abstract class Notification : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public DateTime Time;

		[XmlIgnore]
		[JsonIgnore]
		string description;

		[JsonIgnore]
		public string Description
		{
			get
			{
				return description;
			}

			set
			{
				description = value;
				Notify("Description");
			}
		}

		[XmlIgnore]
		[JsonIgnore]
		string timeString;

		[JsonIgnore]
		public string TimeString
		{
			get
			{
				return timeString;
			}

			set
			{
				timeString = value;
				Notify("TimeString");
			}
		}

		void Notify(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public void Initialise()
		{
			Description = GetDescription();
			timeString = Time.ToStandardString();
		}

		protected abstract string GetDescription();
	}
}
