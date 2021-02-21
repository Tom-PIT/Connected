using System;
using System.Globalization;
using System.Threading;
using TomPIT.ComponentModel;
using TomPIT.Globalization;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareGlobalizationService : MiddlewareObject, IMiddlewareGlobalizationService
	{
		public MiddlewareGlobalizationService(IMiddlewareContext context) : base(context)
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
					var user = Context.Services.Identity.User;

					if (user == null)
						return TimeZoneInfo.Utc;

					try
					{
						var result = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);

						if (result == null)
							return TimeZoneInfo.Utc;
						else
							return result;
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
			return Types.FromUtc(value, Timezone);
		}

		public DateTime ToUtc(DateTime value)
		{
			return Types.ToUtc(value, Timezone);
		}

		public DateTimeOffset FromUtc(DateTimeOffset value)
		{
			return Types.FromUtc(value, Timezone);
		}

		public DateTimeOffset ToUtc(DateTimeOffset value)
		{
			return Types.ToUtc(value, Timezone);
		}

		public Guid Language
		{
			get
			{
				if (!Context.Services.Identity.IsAuthenticated)
					return Guid.Empty;

				var candidate = Context.Services.Identity.User.Language;

				if (candidate != Guid.Empty)
				{
					var result = Context.Tenant.GetService<ILanguageService>().Select(candidate);

					if (result != null && result.Status == LanguageStatus.Visible)
						return result.Token;
				}

				var language = Context.Tenant.GetService<ILanguageService>().Select(Thread.CurrentThread.CurrentUICulture.LCID);

				if (language != null && language.Status == LanguageStatus.Visible)
					return language.Token;

				return Guid.Empty;
			}
		}
		public CultureInfo Culture
		{
			get
			{
				if (Language == Guid.Empty)
					return Thread.CurrentThread.CurrentUICulture;

				var language = Context.Tenant.GetService<ILanguageService>().Select(Language);

				try
				{
					var result = CultureInfo.GetCultureInfo(language.Lcid);

					if (result == null)
						return Thread.CurrentThread.CurrentUICulture;

					return result;
				}
				catch
				{
					return Thread.CurrentThread.CurrentUICulture;
				}
			}
		}

		public DateTimeOffset Now => FromUtc(DateTimeOffset.UtcNow);

		public string GetString(string stringTable, string key, int lcid)
		{
			return GetString(stringTable, key, lcid, true);
		}
		private string GetString(string stringTable, string key, int lcid, bool throwException)
		{
			var descriptor = ComponentDescriptor.StringTable(Context, stringTable);

			descriptor.Validate();

			if (lcid == 0)
				lcid = Culture.LCID;

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
