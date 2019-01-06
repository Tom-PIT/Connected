using System;

namespace TomPIT.Runtime.ApplicationContextServices
{
	internal class TimezoneService : ApplicationContextClient, ITimezoneService
	{
		private TimeZoneInfo _tz = null;

		public TimezoneService(IApplicationContext context) : base(context)
		{
		}

		public TimeZoneInfo Timezone
		{
			get
			{
				if (!Context.Services.Identity.IsAuthenticated)
					return TimeZoneInfo.Utc;
				else
				{
					if (_tz != null)
						return _tz;

					var user = Context.Services.Identity.User;

					if (user == null)
						return TimeZoneInfo.Utc;

					try
					{
						_tz = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);

						if (_tz == null)
							return TimeZoneInfo.Utc;
						else
							return _tz;
					}
					catch
					{
						return TimeZoneInfo.Utc;
					}
				}

			}
		}

		public DateTime FromUtc(DateTime value)
		{
			if (value == DateTime.MinValue)
				return value;

			return DateUtils.FromUtc(value, Timezone);
		}

		public DateTime ToUtc(DateTime value)
		{
			if (value == DateTime.MinValue)
				return value;

			return DateUtils.ToUtc(value, Timezone);
		}
	}
}
