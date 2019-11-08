using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TomPIT.App.Routing
{
	internal class IgnoreRouteMiddleware
	{
		private readonly string[] IgnoreFiles = { "_viewimports.cshtml", "favicon.ico" };
		private readonly string[] IgnoreFolders = { "Assets" };
		private readonly RequestDelegate next;

		public IgnoreRouteMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			if (context.Request.Path.HasValue)
			{
				foreach (var i in IgnoreFiles)
				{
					if (context.Request.Path.Value.EndsWith(i))
					{
						context.Response.StatusCode = (int)HttpStatusCode.NotFound;

						return;
					}
				}

				var path = context.Request.Path.Value.Trim('/');

				foreach (var i in IgnoreFolders)
				{
					var folder = string.Format("{0}/{1}", context.Request.PathBase, i).Trim('/');

					if (path.StartsWith(folder))
					{
						context.Response.StatusCode = (int)HttpStatusCode.NotFound;

						return;
					}
				}
			}


			await next.Invoke(context);
		}
	}
}
