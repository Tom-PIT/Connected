﻿using Microsoft.AspNetCore.Routing;
using System.Net;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Routing;
using TomPIT.Themes;

namespace TomPIT.Configuration
{
	internal class ThemeHandler : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			var ms = Instance.GetService<IMicroServiceService>().Select(Context.GetRouteValue("microService") as string);

			if (ms == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var component = Instance.GetService<IComponentService>().SelectComponent(ms.Token, "Theme", Context.GetRouteValue("theme") as string);

			if (component == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			if (!HasBeenModified(component.Modified))
				return;

			var theme = Instance.GetService<IThemeService>().Compile(ms.Name, component.Name);

			Context.Response.ContentType = "text/css";
			SetModified(component.Modified);

			if (!string.IsNullOrWhiteSpace(theme))
			{
				var buffer = Encoding.UTF8.GetBytes(theme);

				Context.Response.ContentLength = buffer.Length;
				Context.Response.Body.Write(buffer, 0, buffer.Length);
			}
		}
	}
}