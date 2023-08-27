using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
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

	public class RequestLocalizationCookiesMiddleware : Microsoft.AspNetCore.Http.IMiddleware
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

			if (next is not null && !context.Response.HasStarted)
				await next(context);
		}

		private void SetCookie(HttpContext context)
		{
			if (context.Features.Get<IRequestCultureFeature>() is IRequestCultureFeature feature)
			{
				if (context.Response.HasStarted)
				{
					if (MiddlewareDescriptor.Current?.Tenant is ITenant tenant)
					{
						tenant.LogError(nameof(CookieRequestCultureProvider), $"Unable to set culture provider cookie, request already started. ({context.Request.Path})", LogCategories.Middleware);
					}

					return;
				}

				context.Response.Cookies.Append(Provider.CookieName, CookieRequestCultureProvider.MakeCookieValue(feature.RequestCulture));
			}
		}
	}
}

