using System;
using System.Text;
using Microsoft.AspNetCore.Routing;
using TomPIT.Middleware;
using TomPIT.Routing;

namespace TomPIT.App.Globalization
{
	internal class GlobalizationHandler : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			var locale = Context.GetRouteValue("locale") as string;
			var segments = (ClientGlobalizationSegment)Convert.ToInt64(Context.GetRouteValue("segments"));

			var content = MiddlewareDescriptor.Current.Tenant.GetService<IClientGlobalizationService>().LoadData(locale, segments);

			if (!HasBeenModified(DateTime.UtcNow.AddMonths(-3)))
				return;

			Context.Response.ContentType = "application/json";
			SetModified(DateTime.UtcNow);

			if (!string.IsNullOrWhiteSpace(content))
			{
				var buffer = Encoding.UTF8.GetBytes(content);

				Context.Response.ContentLength = buffer.Length;
				Context.Response.Body.WriteAsync(buffer, 0, buffer.Length).Wait();
			}
		}
	}
}
