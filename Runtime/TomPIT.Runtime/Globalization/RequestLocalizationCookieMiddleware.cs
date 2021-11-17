using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

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
				context.Response.Cookies.Append(Provider.CookieName, CookieRequestCultureProvider.MakeCookieValue(feature.RequestCulture));
		}
	}
}
