using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Reflection;

namespace TomPIT.Middleware
{
	public static class MiddlewareExtensions
	{
		public static ServerUrl CreateUrl(this ITenant tenant, string controller, string action)
		{
			return ServerUrl.Create(tenant.Url, controller, action);
		}

		public static T WithConnection<T>(this T operation, IDataConnection connection) where T : IMiddlewareOperation
		{
			if (operation.Context is MiddlewareContext context)
				context.Connection = connection;

			return operation;
		}

		public static T WithConnection<T>(this T operation, IMiddlewareContext context) where T : IMiddlewareOperation
		{
			if (operation.Context is MiddlewareContext op && context is MiddlewareContext mc)
				op.Connection = mc?.Connection;

			return operation;
		}

		public static T WithTransaction<T>(this T operation, IMiddlewareOperation middleware) where T : IMiddlewareOperation
		{
			if (operation is MiddlewareOperation o)
				o.AttachTransaction(middleware);

			return operation;
		}

		internal static void SetContext(this IMiddlewareObject target, IMiddlewareContext context)
		{
			ReflectionExtensions.SetPropertyValue(target, nameof(target.Context), context);
		}
	}
}
