using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TomPIT.Search
{
	internal class SearchResultDescriptorConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(List<ISearchResultDescriptor>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var r = serializer.Deserialize<List<SearchResultDescriptor>>(reader);
			var list = existingValue as List<ISearchResultDescriptor>;

			if (list == null)
				list = new List<ISearchResultDescriptor>();

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