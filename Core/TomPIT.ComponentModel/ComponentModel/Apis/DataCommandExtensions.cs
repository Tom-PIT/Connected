using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace TomPIT.ComponentModel.Apis
{
	public static class DataCommandExtensions
	{
		public static string ToJsonParameterList<T>(this List<T> items, string propertyName)
		{
			var array = new JArray();

			foreach (var item in items)
				array.Add(new JObject { { propertyName, new JValue(item) } });

			return Types.Serialize(array);
		}
	}
}
