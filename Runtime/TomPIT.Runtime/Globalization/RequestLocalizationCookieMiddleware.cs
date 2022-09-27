using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Middleware;

namespace TomPIT.Globalization
{
    public static class RequestLocalizationCookiesMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLocalizationCookies(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestLocalizationCookiesMiddleware>();

            return app;
        }
    }

    public class RequestLocalizationCookiesMiddleware : IMiddleware
    {
        public CookieRequestCultureProvider Provider { get; }

        public RequestLocalizationCookiesMiddleware(IOptions<RequestLocalizationOptions> options)
        {
            Provider = options.Value.RequestCultureProviders.Where(x => x is CookieRequestCultureProvider).Cast<CookieRequestCultureProvider>().FirstOrDefault();
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (Provider is not null)
                SetCookie(context);

            await next(context);
        }

        private void SetCookie(HttpContext context)
        {
            if (context.Features.Get<IRequestCultureFeature>() is IRequestCultureFeature feature)
            {
                //TODO fix exception when logs show what exactly went wrong. Meanwhile, keep the app from crashing.
                try
                {
                    context.Response.Cookies.Append(Provider.CookieName, CookieRequestCultureProvider.MakeCookieValue(feature.RequestCulture));
                }
                catch (InvalidOperationException ex)
                {
                    if (MiddlewareDescriptor.Current?.Tenant is ITenant tenant)
                    {
                        try
                        {
                            tenant.LogError(nameof(CookieRequestCultureProvider), ex.ToString(), LogCategories.Middleware);
                        }
                        catch { }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
