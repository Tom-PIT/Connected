using System;
using System.Collections.Generic;
using System.Text;
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
	}
}
