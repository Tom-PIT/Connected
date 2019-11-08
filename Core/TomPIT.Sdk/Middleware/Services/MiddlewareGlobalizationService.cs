using System;
using System.Threading;
using TomPIT.ComponentModel;
using TomPIT.Globalization;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareGlobalizationService : MiddlewareObject, IMiddlewareGlobalizationService
	{
		private TimeZoneInfo _tz = null;

		public MiddlewareGlobalizationService(IMiddlewareContext context) : base(context)
		{
			if (!context.Services.Identity.IsAuthenticated)
				return;

			var language = Context.Tenant.GetService<ILanguageService>().Select(Thread.CurrentThread.CurrentUICulture.LCID);

			if (language != null && language.Status == LanguageStatus.Visible)
				Language = language.Token;
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

			return Types.FromUtc(value, Timezone);
		}

		public DateTime ToUtc(DateTime value)
		{
			if (value == DateTime.MinValue)
				return value;

			return Types.ToUtc(value, Timezone);
		}

		public Guid Language { get; }

		public string GetString(string stringTable, string key, int lcid)
		{
			return GetString(stringTable, key, lcid, true);
		}
		private string GetString(string stringTable, string key, int lcid, bool throwException)
		{
			var descriptor = ComponentDescriptor.StringTable(Context, stringTable);

			descriptor.Validate();

			if (lcid == 0)
				lcid = Thread.CurrentThread.CurrentUICulture.LCID;

			return Context.Tenant.GetService<ILocalizationService>().GetString(descriptor.MicroService.Name, descriptor.ComponentName, key, lcid, throwException);
		}

		public string GetString(string stringTable, string key)
		{
			return GetString(stringTable, key, 0);
		}

		public string TryGetString(string stringTable, string key)
		{
			return GetString(stringTable, key, 0, false);
		}

		public string TryGetString(string stringTable, string key, int lcid)
		{
			return GetString(stringTable, key, lcid, false);
		}
	}
}
