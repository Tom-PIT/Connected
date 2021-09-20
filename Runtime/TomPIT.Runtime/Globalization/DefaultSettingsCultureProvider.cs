using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Threading.Tasks;
using TomPIT.Configuration;
using TomPIT.Middleware;
using TomPIT.Runtime;

namespace TomPIT.Globalization
{
    class DefaultSettingsCultureProvider : CultureProviderBase, IRequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext) 
        {
            var environment = Shell.GetService<IRuntimeService>().Environment;

            if (environment is RuntimeEnvironment.MultiTenant)
                return Unresolved;

            var value = MiddlewareDescriptor.Current.Tenant.GetService<ISettingService>().GetValue<string>("DefaultCulture", null, null, null);

            if (!string.IsNullOrWhiteSpace(value))
                return Task.FromResult(new ProviderCultureResult(value, value));

            return Unresolved;
        }
    }
}
