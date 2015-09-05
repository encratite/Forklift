using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Forklift
{
	class UnixDateTimeConverter : DateTimeConverterBase
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serialiser)
		{
			writer.WriteValue(Nil.Time.ToUnixTime((DateTime)value));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serialiser)
		{
			return Nil.Time.FromUnixTime((long)reader.Value);
		}
	}
}