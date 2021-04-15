using System;
using Newtonsoft.Json;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Serialization.Converters
{
	internal class DeploymentConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(IClientSysDeployment);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var r = serializer.Deserialize<ClientSysDeployment>(reader);
			var existing = existingValue as ClientSysDeployment;

			if (existing is null)
				existing = new ClientSysDeployment();

			existing.FileSystem = r.FileSystem;

			return existing;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}
	}
}