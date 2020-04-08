using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.Search
{
	public sealed class SearchPropertiesConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(List<ISearchField>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var r = serializer.Deserialize<List<SearchField>>(reader);
			var list = existingValue as List<ISearchField>;

			if (list == null)
				list = new List<ISearchField>();

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