using Microsoft.AspNetCore.Routing;
using TomPIT.Services;

namespace TomPIT.Runtime
{
	public static class RuntimeExtensions
	{
		internal static RouteData GetRouteData(this IHttpRequestOwner owner)
		{
			if (owner == null || owner.HttpRequest == null || owner.HttpRequest.HttpContext == null)
				return null;

			return owner.HttpRequest.HttpContext.GetRouteData();
		}

		internal static bool ContainsQueryParameter(this IHttpRequestOwner owner, string key)
		{
			if (owner.HttpRequest == null)
				return false;

			var q = owner.HttpRequest.Query;

			return q.ContainsKey(key);
		}

		internal static string QueryParameter(this IHttpRequestOwner owner, string key)
		{
			if (!ContainsQueryParameter(owner, key))
				return null;

			var q = owner.HttpRequest.Query;

			if (q.ContainsKey(key))
				return q[key];

			return null;
		}
	}
}
