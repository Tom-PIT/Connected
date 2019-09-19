using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.Ide.TextEditor.Serialization
{
	internal abstract class ListConverter<I, T> : JsonConverter where T : I
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(List<I>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var r = serializer.Deserialize<List<T>>(reader);

			if (!(existingValue is List<I> list))
				list = new List<I>();

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