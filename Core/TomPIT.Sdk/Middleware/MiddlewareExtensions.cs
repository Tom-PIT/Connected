using TomPIT.Connectivity;
using TomPIT.Reflection;

namespace TomPIT.Middleware
{
	public static class MiddlewareExtensions
	{
		public static ServerUrl CreateUrl(this ITenant tenant, string controller, string action)
		{
			return ServerUrl.Create(tenant.Url, controller, action);
		}

		internal static void SetContext(this IMiddlewareObject target, IMiddlewareContext context)
		{
			ReflectionExtensions.SetPropertyValue(target, nameof(target.Context), context);
		}
	}
}
