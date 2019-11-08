using System.Collections;
using System.Collections.Generic;
using TomPIT.Exceptions;

namespace TomPIT.Collections
{
	public static class CollectionExtensions
	{
		public static Dictionary<string, string> ToDictionary(this IEnumerable items, string keyProperty, string valueProperty)
		{
			var result = new Dictionary<string, string>();

			foreach (var item in items)
			{
				var keyPropertyInfo = item.GetType().GetProperty(keyProperty);
				var valuePropertyInfo = item.GetType().GetProperty(valueProperty);

				if (keyPropertyInfo == null)
					throw new RuntimeException($"SR.ErrPropertyNotFound ({keyProperty})");

				if (valuePropertyInfo == null)
					throw new RuntimeException($"SR.ErrPropertyNotFound ({valueProperty})");

				var keyValue = Types.Convert<string>(keyPropertyInfo.GetValue(item));
				var valueValue = Types.Convert<string>(valuePropertyInfo.GetValue(item));

				result.Add(keyValue, valueValue);
			}

			return result;
		}
	}
}
