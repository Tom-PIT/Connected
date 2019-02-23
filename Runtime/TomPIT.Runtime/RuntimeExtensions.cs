using Microsoft.AspNetCore.Http;
using System;
using TomPIT.ComponentModel;
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

		public static bool IsMicroServiceSupported(this ISysConnection connection, Guid microService)
		{
			var ms = connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return false;

			return Instance.ResourceGroupExists(ms.ResourceGroup);
		}
	}
}
