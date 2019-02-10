using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TomPIT.IoT
{
	internal class IoTStateConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(List<IIoTFieldStateModifier>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var r = serializer.Deserialize<List<IoTFieldStateModifier>>(reader);
			var list = existingValue as List<IIoTFieldStateModifier>;

			if (list == null)
				list = new List<IIoTFieldStateModifier>();

			foreach (var i in r)
				list.Add(i);

			return list;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}
	}
}