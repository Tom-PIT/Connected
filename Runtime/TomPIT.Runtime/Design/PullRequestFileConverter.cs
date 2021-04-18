using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.Design
{
	public class PullRequestFileConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(List<IPullRequestFile>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var r = serializer.Deserialize<List<PullRequestFile>>(reader);
			var list = existingValue as List<IPullRequestFile>;

			if (list == null)
				list = new List<IPullRequestFile>();

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