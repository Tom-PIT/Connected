using System.Net;
using System.Text;
using Microsoft.AspNetCore.Routing;
using TomPIT.ComponentModel;
using TomPIT.Routing;
using TomPIT.UI.Theming;

namespace TomPIT.App.UI.Theming
{
	internal class ThemeHandler : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			var ms = Instance.Tenant.GetService<IMicroServiceService>().Select(Context.GetRouteValue("microService") as string);

			if (ms == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var component = Instance.Tenant.GetService<IComponentService>().SelectComponent(ms.Token, "Theme", Context.GetRouteValue("theme") as string);

			if (component == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			if (!HasBeenModified(component.Modified))
				return;

			var theme = Instance.Tenant.GetService<IThemeService>().Compile(ms.Name, component.Name);

			Context.Response.ContentType = "text/css";
			SetModified(component.Modified);

			if (!string.IsNullOrWhiteSpace(theme))
			{
				var buffer = Encoding.UTF8.GetBytes(theme);

				Context.Response.ContentLength = buffer.Length;
				Context.Response.Body.WriteAsync(buffer, 0, buffer.Length).Wait();
			}
		}
	}
}
