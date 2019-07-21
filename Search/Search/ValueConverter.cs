using System;
using System.Globalization;

namespace TomPIT.Search
{
	internal static class ValueConverter
	{
		public static string Convert<T>(object value)
		{
			if (value == null || value == DBNull.Value)
				return null;

			if (typeof(T) == typeof(DateTime))
			{
				DateTime dt;

				if (value is DateTime)
					dt = (DateTime)value;
				else if (!DateTime.TryParse(value.ToString(), out dt))
					return null;

				if (dt == DateTime.MinValue)
					return null;

				return dt.Ticks.ToString(CultureInfo.InvariantCulture);

			}

			return Types.Convert<string>(Types.Convert<T>(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
		}
	}
}