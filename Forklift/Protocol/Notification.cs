using System;
using Newtonsoft.Json;

namespace Forklift
{
	public abstract class Notification
	{
		[JsonIgnore]
		public DateTime Time;
	}
}
