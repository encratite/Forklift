using System;

namespace Forklift
{
	public static class Time
	{
		private static DateTime _Epoch = new DateTime(1970, 1, 1);

		public static string Timestamp()
		{
			return DateTime.Now.ToStandardString();
		}

		public static string ToStandardString(this DateTime time)
		{
			return time.ToString("yyyy-MM-dd HH:mm:ss");
		}

		public static long ToUnixTime(this DateTime time)
		{
			return (long)(time - _Epoch).TotalSeconds;
		}

		public static DateTime FromUnixTime(this long timestamp)
		{
			return _Epoch.AddSeconds(timestamp);
		}

		public static long UnixTime()
		{
			return DateTime.UtcNow.ToUnixTime();
		}
	}
}
