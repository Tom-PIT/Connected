using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Net;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Runtime.ApplicationContextServices
{
	internal class Routing : HttpRequestClient, IRoutingService
	{
		public Routing(IApplicationContext context, IHttpRequestOwner request) : base(context, request)
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
			return Context.GetServerContext().GetService<IInstanceEndpointService>().Url(type, verbs);
		}

		public string Resource(IUrlHelper helper, Guid blob)
		{
			var b = Context.GetServerContext().GetService<IStorageService>().Select(blob);

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

			var u = Context.GetServerContext().GetService<IUserService>().Select(user.ToString());

			if (u == null || u.Avatar == Guid.Empty)
				return null;

			var blob = Context.GetServerContext().GetService<IStorageService>().Select(u.Avatar);

			if (blob == null)
				return null;

			return string.Format("~/sys/avatar/{0}/{1}", u.Avatar, blob.Version);
		}
	}
}
