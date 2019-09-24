using TomPIT.Connectivity;

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
			var property = target.GetType().GetProperty("Context");

			if (property.SetMethod == null)
				return;

			property.SetMethod.Invoke(target, new object[] { context });
		}
	}
}
