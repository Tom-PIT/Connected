using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.Data
{
	public static class DataCommandExtensions
	{
		public static string ToJsonParameterList<T>(this List<T> items, string propertyName)
		{
			var array = new JArray();

			foreach (var item in items)
				array.Add(new JObject { { propertyName, new JValue(item) } });

			return Serializer.Serialize(array);
		}

		public static List<string> Tokenize(this List<string> items)
		{
			return Tokenize(items, ',');
		}
		public static List<string> Tokenize(this List<string> items, char separator)
		{
			var r = new List<string>();

			foreach (var item in items)
			{
				if (string.IsNullOrWhiteSpace(item))
					continue;

				var tokens = item.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);

				foreach (var token in tokens)
				{
					if (r.Contains(token, StringComparer.OrdinalIgnoreCase))
						continue;

					r.Add(token);
				}
			}

			return r;
		}
	}
}
