using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.Globalization
{
	internal class IdentityCultureProvider : IRequestCultureProvider
	{
		public Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
		{
			if (httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
				return SettingCulture(httpContext);

			var identity = httpContext.User.Identity as Identity;

			if(identity == null || identity.User == null)
				return SettingCulture(httpContext);

			var languageToken = identity.User.Language;

			if(languageToken == Guid.Empty)
				return SettingCulture(httpContext);

			var endpoint = Shell.GetService<IConnectivityService>().Select(identity.Endpoint);

			if (endpoint == null)
				return SettingCulture(httpContext);

			var language = endpoint.GetService<ILanguageService>().Select(languageToken);

			if(language == null || language.Status == LanguageStatus.Hidden || language.Lcid == 0)
				return SettingCulture(httpContext);

			var culture = CultureInfo.GetCultureInfo(language.Lcid);

			if (culture == null)
				return SettingCulture(httpContext);

			return Task.FromResult(new ProviderCultureResult(culture.Name));
		}

		private Task<ProviderCultureResult> SettingCulture(HttpContext httpContext)
		{
			var environment = Shell.GetService<IRuntimeService>().Environment;

			if (environment == RuntimeEnvironment.MultiTenant)
				return Task.FromResult<ProviderCultureResult>(null);

			var value = Instance.GetService<ISettingService>().GetValue<string>(Guid.Empty, "DefaultCulture");

			if (!string.IsNullOrWhiteSpace(value))
				return Task.FromResult(new ProviderCultureResult(value));

			return Task.FromResult<ProviderCultureResult>(null);
		}
	}
}
