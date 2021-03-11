using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TomPIT.App.UI;
using TomPIT.Controllers;
using TomPIT.Middleware;
using TomPIT.Navigation;
using TomPIT.Runtime;
using TomPIT.UI;

namespace TomPIT.App.Controllers
{
	[AllowAnonymous]
	public class ViewController: ServerController
	{
		[HttpPost]
		[HttpGet]
		public async Task Invoke()
		{
			if (string.IsNullOrWhiteSpace(HttpContext.Request.Path.ToString().Trim('/')))
				HttpContext.Request.Path = "/home";

			if (Redirect(HttpContext))
				return;

			var ve = HttpContext.RequestServices.GetService(typeof(IViewEngine)) as ViewEngine;

			ve.Context = HttpContext;

			await ve.Render(HttpContext.Request.Path);
		}

		private static bool Redirect(HttpContext context)
		{
			if (context.Request.Path.ToString().StartsWith("/home"))
			{
				if (HomeResolved(context))
					return true;
			}

			var routes = new RouteValueDictionary();

			using var route = MiddlewareDescriptor.Current.Tenant.GetService<INavigationService>().MatchRoute(context.Request.Path, routes);

			if (route is ISiteMapRedirectRoute redirect)
			{
				context.Response.StatusCode = (int)HttpStatusCode.Redirect;
				context.Response.Redirect(redirect.RedirectUrl(routes));

				return true;
			}

			return false;
		}

		private static bool HomeResolved(HttpContext context)
		{
			var runtimes = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceRuntimeService>().QueryRuntimes();
			var resolvedHomeUrls = new List<IRuntimeUrl>();

			foreach (var middleware in runtimes)
			{
				var homeUrl = middleware.ResolveUrl(RuntimeUrlKind.Default);

				if (homeUrl != null)
					resolvedHomeUrls.Add(homeUrl);
			}

			if (resolvedHomeUrls.Count == 0)
				return false;

			var winner = resolvedHomeUrls.OrderByDescending(f => f.Weight).First();

			context.Response.StatusCode = (int)HttpStatusCode.Redirect;
			context.Response.Redirect(winner.Url);

			return true;
		}
	}
}
