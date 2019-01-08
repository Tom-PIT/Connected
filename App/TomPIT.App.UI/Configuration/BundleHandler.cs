using Microsoft.AspNetCore.Routing;
using System.Net;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Resources;
using TomPIT.Routing;

namespace TomPIT.Configuration
{
	internal class BundleHandler : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			var ms = Instance.GetService<IMicroServiceService>().Select(Context.GetRouteValue("microService") as string);

			if (ms == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			var component = Instance.GetService<IComponentService>().SelectComponent(ms.Token, "Bundle", Context.GetRouteValue("bundle") as string);

			if (component == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			if (!HasBeenModified(component.Modified))
				return;

			var bundle = Instance.GetService<IResourceService>().Bundle(ms.Name, component.Name);

			Context.Response.ContentType = "application/javascript";

			SetModified(component.Modified);

			if (!string.IsNullOrWhiteSpace(bundle))
			{
				var buffer = Encoding.UTF8.GetBytes(bundle);

				Context.Response.ContentLength = buffer.Length;
				Context.Response.Body.Write(buffer, 0, buffer.Length);
			}
		}
	}
}
