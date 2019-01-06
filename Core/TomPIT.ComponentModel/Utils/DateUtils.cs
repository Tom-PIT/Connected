using System;

namespace TomPIT
{
	public static class DateUtils
	{
		public static DateTime FromUtc(DateTime value, TimeZoneInfo timeZone)
		{
			if (value == DateTime.MinValue)
				return value;

			if (timeZone == null || timeZone == TimeZoneInfo.Utc)
				return value;
			else
				return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), timeZone);
		}

		public static DateTime ToUtc(DateTime value, TimeZoneInfo timeZone)
		{
			if (value == DateTime.MinValue)
				return value;

			if (timeZone == null || timeZone == TimeZoneInfo.Utc)
				return value;
			else
				return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), timeZone);
		}
	}
}
