using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using TomPIT.Environment;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Services.Context
{
	internal class ContextRoutingService : ContextClient, IContextRoutingService
	{
		public ContextRoutingService(IExecutionContext context) : base(context)
		{
		}

		public string Absolute(string url)
		{
			if (!IsRelative(url))
				return url;

			try
			{
				return new UriBuilder(url).Uri.ToString();
			}
			catch
			{
				return url;
			}
		}

		private bool IsRelative(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return false;

			try
			{
				if (Uri.TryCreate(url, UriKind.Relative, out Uri uri))
					return true;
			}
			catch { return false; }

			return false;
		}

		public string GetServer(InstanceType type, InstanceVerbs verbs)
		{
			return Context.Connection().GetService<IInstanceEndpointService>().Url(type, verbs);
		}

		public string Resource(IUrlHelper helper, Guid blob)
		{
			var b = Context.Connection().GetService<IStorageService>().Select(blob);

			if (b == null)
				return null;

			return helper.RouteUrl("sys.resource", new
			{
				blob,
				version = b.Version
			});
		}

		public string Avatar(Guid user)
		{
			if (user == Guid.Empty)
				return null;

			var u = Context.Connection().GetService<IUserService>().Select(user.ToString());

			if (u == null || u.Avatar == Guid.Empty)
				return null;

			var blob = Context.Connection().GetService<IStorageService>().Select(u.Avatar);

			if (blob == null)
				return null;

			return string.Format("~/sys/avatar/{0}/{1}", u.Avatar, blob.Version);
		}

		public void NotFound()
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpRequestNotAvailable);

			Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
		}

		public void Forbidden()
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpRequestNotAvailable);

			Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
		}

		public void Redirect(string url)
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpRequestNotAvailable);

			Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
			Shell.HttpContext.Response.Redirect(url, false);
		}

		public void BadRequest()
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpRequestNotAvailable);

			Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
		}

		public string ApplicationUrl(string route)
		{
			var appUrl = GetServer(InstanceType.Application, InstanceVerbs.Get);

			if (appUrl == null)
				throw new RuntimeException(SR.ErrNoAppServer);

			return string.Format("{0}/{1}", appUrl, route);
		}
	}
}
