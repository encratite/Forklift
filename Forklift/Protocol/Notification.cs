using System;
using System.Xml.Serialization;
using System.ComponentModel;

using Newtonsoft.Json;

using Nil;

namespace Forklift
{
	public enum NotificationType
	{
		Routine,
		Information,
		Error,
	}

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

		[XmlIgnore]
		[JsonIgnore]
		string colour;

		[JsonIgnore]
		public string Colour
		{
			get
			{
				return colour;
			}

			set
			{
				colour = value;
				Notify("Colour");
			}
		}

		[XmlIgnore]
		[JsonIgnore]
		string imageString;

		[JsonIgnore]
		public string ImageString
		{
			get
			{
				return imageString;
			}

			set
			{
				imageString = value;
				Notify("ImageString");
			}
		}

		void Notify(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public void Initialise(bool isNew)
		{
			Description = GetDescription();
			TimeString = Time.ToStandardString();
			Colour = isNew ? "Green" : "Black";
			ImageString = GetImageString();
		}

		protected abstract string GetDescription();
		protected abstract string GetImageString();

		public abstract NotificationType GetNotificationType();
	}
}
