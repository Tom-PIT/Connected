using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT.Globalization
{
    internal class IdentityCultureProvider : CultureProviderBase, IRequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            var user = httpContext.User;

            if (user is null)
                return Unresolved;

            if (user.Identity is not Identity identity)
                return Unresolved;

            if (!identity.IsAuthenticated)
                return Unresolved;

            var identityUser = identity.User;

            if (identityUser is null)
                return Unresolved;

            var languageToken = identity.User.Language;

            if (languageToken == default)
                return Unresolved;

            var endpoint = Shell.GetService<IConnectivityService>().SelectTenant(identity.Endpoint);

            if (endpoint is null)
                return Unresolved;

            var language = endpoint.GetService<ILanguageService>().Select(languageToken);

            if (language is null || language.Status == LanguageStatus.Hidden || language.Lcid == 0)
                return Unresolved;

            var culture = CultureInfo.GetCultureInfo(language.Lcid);

            if (culture is null)
                return Unresolved;

            return Task.FromResult(new ProviderCultureResult(culture.Name, culture.Name));
        }
    }
}
