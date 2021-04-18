using System;
using Newtonsoft.Json;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Serialization.Converters
{
	internal class DeploymentFileSystemConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(IClientSysFileSystemDeployment);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var r = serializer.Deserialize<FileSystemDeployment>(reader);
			var existing = existingValue as FileSystemDeployment;

			if (existing is null)
				existing = new FileSystemDeployment();

			existing.Enabled = r.Enabled;
			existing.Path = r.Path;

			return existing;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}
	}
}