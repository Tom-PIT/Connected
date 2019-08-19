using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using TomPIT.Routing;
using TomPIT.Storage;

namespace TomPIT.Configuration
{
	internal class MediaHandler : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			var blob = Connection.GetService<IStorageService>().Select((Context.GetRouteValue("id") as string).AsGuid());

			if (blob == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;

				return;
			}

			if (!HasBeenModified(blob.Modified))
				return;

			var content = Connection.GetService<IStorageService>().Download(blob.Token);

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
