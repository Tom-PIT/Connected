﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using TomPIT.UI;

namespace TomPIT.Configuration
{
	internal static class Routing
	{
		public static void Register(IRouteBuilder routes)
		{

			routes.MapRoute("sys.api", "sys/api/invoke", new { controller = "Api", action = "Invoke" });
			routes.MapRoute("sys.partial", "sys/api/partial", new { controller = "Api", action = "Partial" });

			routes.MapRoute("sys/themes/{microService}/{theme}", (t) =>
			{
				new ThemeHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.MapRoute("sys/globalize/{locale}/{segments}", (t) =>
			{
				new GlobalizationHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.MapRoute("sys/bundles/{microService}/{bundle}", (t) =>
			{
				new BundleHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.MapRoute("{*.}", (t) =>
			{
				if (string.IsNullOrWhiteSpace(t.Request.Path.ToString().Trim('/')))
					t.Request.Path = "/home";

				var ve = t.RequestServices.GetService(typeof(IViewEngine)) as ViewEngine;

				ve.Context = t;

				ve.Render(t.Request.Path);

				return Task.CompletedTask;
			});
		}
	}
}