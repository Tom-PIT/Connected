using System.Net;
using Microsoft.AspNetCore.Routing;
using TomPIT.Storage;

namespace TomPIT.Routing
{
	public class MediaHandler : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			var blob = Instance.GetService<IStorageService>().Select((Context.GetRouteValue("id") as string).AsGuid());

			if (blob == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			if (!HasBeenModified(blob.Modified))
				return;

			var content = Instance.GetService<IStorageService>().Download(blob.Token);

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
