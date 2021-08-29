using System;
using System.Net;
using Microsoft.AspNetCore.Routing;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Routing;
using TomPIT.Storage;

namespace TomPIT.App.Resources
{
	internal class StaticHandler : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			var msRoute = Context.GetRouteValue("microService") as string;
			var ms = Tenant.GetService<IMicroServiceService>().Select(msRoute);
			var path = Context.GetRouteValue("path") as string;

			if (ms is null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var tokens = path.Split('/');
			var component = Tenant.GetService<IComponentService>().SelectComponent(ms.Token, ComponentCategories.Static, tokens[^1]);

			if (component is null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			if (!HasBeenModified(component.Modified))
				return;

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) as IStaticEmbeddedResource;

			if (config.Blob == Guid.Empty)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var file = Tenant.GetService<IStorageService>().Select(config.Blob);

			if (file is null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			Context.Response.ContentType = file.ContentType;

			SetModified(component.Modified);

			var content = Tenant.GetService<IStorageService>().Download(file.Token);

			if (content is null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			Context.Response.ContentLength = content.Content.Length;
			Context.Response.Body.WriteAsync(content.Content, 0, content.Content.Length).Wait();
			Context.Response.CompleteAsync().Wait();
		}
	}
}
