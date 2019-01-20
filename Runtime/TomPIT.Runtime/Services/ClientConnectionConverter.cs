using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TomPIT.Services
{
	internal class ClientConnectionConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(List<IClientSysConnection>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var r= serializer.Deserialize<List<ClientSysConnection>>(reader);
			var list = existingValue as List<IClientSysConnection>;

			foreach (var i in r)
				list.Add(i);

			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}
	}
}