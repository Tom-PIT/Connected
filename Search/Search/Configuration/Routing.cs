﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Search.Configuration
{
	internal static class Routing
	{
		public static void Register(IRouteBuilder routes)
		{
			routes.MapRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });
			routes.MapRoute("default", "{controller}/{action}");
		}
	}
}