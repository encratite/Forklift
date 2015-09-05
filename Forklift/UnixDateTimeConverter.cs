using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Forklift
{
	class UnixDateTimeConverter : DateTimeConverterBase
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serialiser)
		{
			writer.WriteValue(Time.ToUnixTime((DateTime)value));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serialiser)
		{
			return Time.FromUnixTime((long)reader.Value);
		}
	}
}