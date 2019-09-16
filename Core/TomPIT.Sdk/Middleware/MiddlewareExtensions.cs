using TomPIT.Connectivity;

namespace TomPIT.Middleware
{
	public static class MiddlewareExtensions
	{
		public static ServerUrl CreateUrl(this ITenant tenant, string controller, string action)
		{
			return ServerUrl.Create(tenant.Url, controller, action);
		}
	}
}
