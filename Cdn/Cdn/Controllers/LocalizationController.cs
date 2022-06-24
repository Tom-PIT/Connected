using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using TomPIT.Cdn.Clients;
using TomPIT.Configuration;
using TomPIT.Controllers;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Globalization;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Security;

namespace TomPIT.Cdn.Controllers
{
	[Authorize(AuthenticationSchemes = "TomPIT")]
	public class LocalizationController : ServerController
	{
		[HttpPost]
		public ActionResult<LocalizedStringValue> Localize()
		{
			var body = FromBody();
			var microservice = body.Required<string>("microservice");
			var stringTable = body.Required<string>("stringTable");
			var key = body.Required<string>("stringKey");
			var identity = body.Optional<string>("identity", Guid.Empty.ToString());

			var defaultCultureSetting = MiddlewareDescriptor.Current.Tenant.GetService<ISettingService>().GetValue<string>("DefaultCulture", null, null, null);

			var culture = GetCulture(defaultCultureSetting, CultureInfo.InvariantCulture);
			            
            var user = MiddlewareDescriptor.Current.Tenant.GetService<IUserService>().Select(identity);

			if (user is not null) 
			{
				var language = MiddlewareDescriptor.Current.Tenant.GetService<ILanguageService>().Select(user.Language);
				if(language is not null) 
				{
					culture = GetCulture(language.Lcid, culture);	
				}
			}

			var localizationService = MiddlewareDescriptor.Current.Tenant.GetService<ILocalizationService>();

			var localizedValue = localizationService.GetString(microservice, stringTable, key, culture.LCID, false);

			if (localizedValue is null)
				return NotFound();

			return new LocalizedStringValue
			{
				LCID = culture.LCID,
				Value = localizedValue
			};
		}

		private CultureInfo GetCulture(string name, CultureInfo fallback) 
		{
			try
			{
				return CultureInfo.GetCultureInfo(name);
			}
			catch { }

			return fallback;
		}

		private CultureInfo GetCulture(int lcid, CultureInfo fallback)
		{
			try
			{
				return CultureInfo.GetCultureInfo(lcid);
			}
			catch { }

			return fallback;
		}

		public class LocalizedStringValue 
		{
			public int LCID { get; set; }
			public string Value { get; set; }
		}
	}
}
