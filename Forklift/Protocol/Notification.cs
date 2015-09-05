using System;
using System.ComponentModel;
using System.Xml.Serialization;
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

		public DateTime Time { get; set; }

		[JsonIgnore]
		public string Description
		{
			get
			{
				return _Description;
			}

			set
			{
				_Description = value;
				Notify("Description");
			}
		}

		[JsonIgnore]
		public string TimeString
		{
			get
			{
				return _TimeString;
			}

			set
			{
				_TimeString = value;
				Notify("TimeString");
			}
		}

		[JsonIgnore]
		public string Colour
		{
			get
			{
				return _Colour;
			}

			set
			{
				_Colour = value;
				Notify("Colour");
			}
		}

		[JsonIgnore]
		public string ImageString
		{
			get
			{
				return _ImageString;
			}

			set
			{
				_ImageString = value;
				Notify("ImageString");
			}
		}

		[XmlIgnore]
		[JsonIgnore]
		private string _Description;

		[XmlIgnore]
		[JsonIgnore]
		private string _TimeString;

		[XmlIgnore]
		[JsonIgnore]
		private string _Colour;

		[XmlIgnore]
		[JsonIgnore]
		private string _ImageString;

		public void Initialise(bool isNew)
		{
			Description = GetDescription();
			TimeString = Time.ToStandardString();
			Colour = isNew ? "#FF1ABF22" : "Black";
			ImageString = GetImageString();
		}

		public abstract NotificationType GetNotificationType();

		protected abstract string GetDescription();

		protected abstract string GetImageString();

		private void Notify(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
