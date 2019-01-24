using Microsoft.AspNetCore.Http;
using TomPIT.Connectivity;
using TomPIT.Routing;

namespace TomPIT
{
	public static class RuntimeExtensions
	{
		internal static bool ContainsQueryParameter(this HttpContext context, string key)
		{
			if (context == null)
				return false;

			var q = context.Request.Query;

			return q.ContainsKey(key);
		}

		internal static string QueryParameter(this HttpContext context, string key)
		{
			if (!ContainsQueryParameter(context, key))
				return null;

			var q = context.Request.Query;

			if (q.ContainsKey(key))
				return q[key];

			return null;
		}

		public static ServerUrl CreateUrl(this ISysConnection connection, string baseUrl, string microService, string api, string operation)
		{
			return new ApiUrl(baseUrl, microService, api, operation);
		}

	}
}
