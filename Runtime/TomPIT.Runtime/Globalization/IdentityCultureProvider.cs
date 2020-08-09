using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Security;

namespace TomPIT.Globalization
{
	internal class IdentityCultureProvider : IRequestCultureProvider
	{
		public Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
		{
			if (httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
				return SettingCulture(httpContext);

			var identity = httpContext.User.Identity as Identity;

			if (identity == null || identity.User == null)
				return SettingCulture(httpContext);

			var languageToken = identity.User.Language;

			if (languageToken == Guid.Empty)
				return SettingCulture(httpContext);

			var endpoint = Shell.GetService<IConnectivityService>().SelectTenant(identity.Endpoint);

			if (endpoint == null)
				return SettingCulture(httpContext);

			var language = endpoint.GetService<ILanguageService>().Select(languageToken);

			if (language == null || language.Status == LanguageStatus.Hidden || language.Lcid == 0)
				return SettingCulture(httpContext);

			var culture = CultureInfo.GetCultureInfo(language.Lcid);

			if (culture == null)
				return SettingCulture(httpContext);

			return Task.FromResult(new ProviderCultureResult(culture.Name, culture.Name));
		}

		private Task<ProviderCultureResult> SettingCulture(HttpContext httpContext)
		{
			var environment = Shell.GetService<IRuntimeService>().Environment;

			if (environment == RuntimeEnvironment.MultiTenant)
				return Task.FromResult<ProviderCultureResult>(null);

			var value = MiddlewareDescriptor.Current.Tenant.GetService<ISettingService>().GetValue<string>("DefaultCulture", null, null, null);

			if (!string.IsNullOrWhiteSpace(value))
				return Task.FromResult(new ProviderCultureResult(value, value));

			return Task.FromResult<ProviderCultureResult>(null);
		}
	}
}
