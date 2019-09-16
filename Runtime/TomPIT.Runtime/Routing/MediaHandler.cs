using System;
using System.Net;
using Microsoft.AspNetCore.Routing;
using TomPIT.Storage;

namespace TomPIT.Routing
{
	public class MediaHandler : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			var blob = Instance.Tenant.GetService<IStorageService>().Select(new Guid((Context.GetRouteValue("id") as string)));

			if (blob == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			if (!HasBeenModified(blob.Modified))
				return;

			var content = Instance.Tenant.GetService<IStorageService>().Download(blob.Token);

			Context.Response.ContentType = blob.ContentType;

			SetModified(blob.Modified);

			if (content != null)
			{
				Context.Response.ContentLength = content.Content.Length;
				Context.Response.Body.Write(content.Content, 0, content.Content.Length);
			}
		}
	}
}
