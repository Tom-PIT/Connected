using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Globalization
{
	internal class DomainCultureProvider : CultureProviderBase, IRequestCultureProvider
	{
		public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
		{
			var domain = httpContext.Request.Host.ToString().Split('.').LastOrDefault()?.Split(':').FirstOrDefault();

			if (string.IsNullOrWhiteSpace(domain))
				return Unresolved;

			var languageService = MiddlewareDescriptor.Current.Tenant.GetService<ILanguageService>();

			var language = languageService.Match(domain);

			if (language is null)
				return Unresolved;

			var culture = CultureInfo.GetCultureInfo(language.Lcid);

			if (culture is null)
				return Unresolved;

			return Task.FromResult(new ProviderCultureResult(culture.Name, culture.Name));
		}
	}
}
