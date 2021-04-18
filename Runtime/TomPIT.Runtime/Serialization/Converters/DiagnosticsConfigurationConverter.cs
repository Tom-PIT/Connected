using System;
using Newtonsoft.Json;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Serialization.Converters
{
	internal class DiagnosticsConfigurationConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(IDiagnosticsConfiguration);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var r = serializer.Deserialize<DiagnosticsConfiguration>(reader);
			var existing = existingValue as DiagnosticsConfiguration;

			existing.DumpEnabled = r.DumpEnabled;

			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}
	}
}