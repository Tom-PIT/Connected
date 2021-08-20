using System.Net;
using System.Text;
using Microsoft.AspNetCore.Routing;
using TomPIT.ComponentModel;
using TomPIT.Routing;

namespace TomPIT.App.Resources
{
	internal class BundleHandler : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			var msRoute = Context.GetRouteValue("microService") as string;
			var bundleRoute = Context.GetRouteValue("bundle") as string;

			var ms = Tenant.GetService<IMicroServiceService>().Select(msRoute);

			if (ms == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var component = Tenant.GetService<IComponentService>().SelectComponent(ms.Token, ComponentCategories.ScriptBundle, bundleRoute);

			if (component == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			if (!HasBeenModified(component.Modified))
				return;

			var bundle = Tenant.GetService<IResourceService>().Bundle(ms.Name, component.Name);

			Context.Response.ContentType = "application/javascript";

			SetModified(component.Modified);

			if (!string.IsNullOrWhiteSpace(bundle))
			{
				var buffer = Encoding.UTF8.GetBytes(bundle);

				Context.Response.ContentLength = buffer.Length;
				Context.Response.Body.WriteAsync(buffer, 0, buffer.Length).Wait();

				Context.Response.CompleteAsync().Wait();
			}
		}
	}
}
