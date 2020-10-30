using System;
using System.Globalization;

namespace TomPIT
{
	public static class StringExtensions
	{
		public const string PreciseDateFormat = "yyyy-MM-dd HH:mm:ss.fffffff";

		public static string WithMilliseconds(this DateTime value)
		{
			return value.ToString(PreciseDateFormat, CultureInfo.InvariantCulture);
		}

		public static string ToCamelCase(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return value;

			return $"{char.ToLowerInvariant(value[0])}{value.Substring(1)}";
		}

		public static string ToPascalCase(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return value;

			return $"{char.ToUpperInvariant(value[0])}{value.Substring(1)}";
		}
	}
}
