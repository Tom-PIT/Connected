using Microsoft.AspNetCore.Routing;
using System.Net;
using TomPIT.Storage;

namespace TomPIT.Routing
{
	internal class AvatarRouteHandler : RouteHandlerBase
	{
		protected override void OnProcessRequest()
		{
			/*
			 * on multi tenant environments (development, management) SysContext
			 * should be provided as part of authenticated request. On single tenant
			 * we've got only one server connection.
			 * since this resource is not under authentication we don't have to
			 * deal with authentication cookie.
			 */
			var ctx = Connection ?? Instance.Connection;

			var blob = Context.GetRouteValue("token").ToString().AsGuid();
			var version = Context.GetRouteValue("version").ToString().AsInt();

			var b = ctx.GetService<IStorageService>().Select(blob);

			if (b == null || b.Version != version)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			if (!HasBeenModified(b.Modified))
				return;

			Context.Response.ContentType = b.ContentType;

			SetModified(b.Modified);

			var content = ctx.GetService<IStorageService>().Download(blob);

			if (content != null && content.Content != null && content.Content.Length > 0)
			{
				Context.Response.ContentLength = content.Content.Length;
				Context.Response.Body.Write(content.Content, 0, content.Content.Length);
			}
		}
	}
}
