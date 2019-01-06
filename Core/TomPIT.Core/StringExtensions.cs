using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TomPIT
{
	public static class StringExtensions
	{
		public const string PreciseDateFormat = "yyyy-MM-dd HH:mm:ss.fffffff";

		public static string WithMilliseconds(this DateTime value)
		{
			return value.ToString(PreciseDateFormat, CultureInfo.InvariantCulture);
		}
	}
}
