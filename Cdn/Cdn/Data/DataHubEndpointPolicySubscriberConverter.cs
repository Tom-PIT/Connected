using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.Cdn.Data
{
	public class DataHubEndpointPolicySubscriberConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(IDataHubEndpointPolicySubscriber);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var r = serializer.Deserialize<JObject>(reader);
			var descriptor = new DataHubEndpointPolicySubscriber();
			var arguments = r.Optional<JObject>("arguments", null);

			descriptor.Name = r.Required<string>("name");
			descriptor.Arguments = arguments == null ? string.Empty : Serializer.Serialize(arguments);

			return descriptor;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}
	}
}