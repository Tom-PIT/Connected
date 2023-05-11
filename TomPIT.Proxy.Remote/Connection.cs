using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Proxy.Remote
{
	internal static class Connection
	{
		public static ServerUrl CreateUrl(string controller, string action)
		{
			return MiddlewareDescriptor.Current.Tenant.CreateUrl(controller, action);
		}

		public static void Post(string url, object args)
		{
			MiddlewareDescriptor.Current.Tenant.Post(url, args);
		}

		public static T Post<T>(string url, object args)
		{
			return MiddlewareDescriptor.Current.Tenant.Post<T>(url, args);
		}

		public static T Get<T>(string url)
		{
			return MiddlewareDescriptor.Current.Tenant.Get<T>(url);
		}
	}
}
