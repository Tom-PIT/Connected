﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.Design
{
	public class PullRequestComponentConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(List<IPullRequestComponent>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var r = serializer.Deserialize<List<PullRequestComponent>>(reader);
			var list = existingValue as List<IPullRequestComponent>;

			if (list == null)
				list = new List<IPullRequestComponent>();

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