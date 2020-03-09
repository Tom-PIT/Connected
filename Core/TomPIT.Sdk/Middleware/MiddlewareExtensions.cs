using System;
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

		[Obsolete("Use WithContext extension method.")]
		public static T WithConnection<T>(this T operation, IDataConnection connection) where T : IMiddlewareOperation
		{
			return operation;
		}

		[Obsolete("Use WithContext extension method.")]
		public static T WithConnection<T>(this T operation, IMiddlewareContext context) where T : IMiddlewareOperation
		{
			return WithContext(operation, context);
		}

		public static T WithContext<T>(this T operation, IMiddlewareContext context) where T : IMiddlewareOperation
		{
			if (operation.Context is MiddlewareContext op && context is MiddlewareContext mc)
			{
				op.Owner = mc;

				if (operation is MiddlewareOperation mop && mc.Transaction != null)
					mop.Transaction = mc.Transaction;
			}

			return operation;
		}
		[Obsolete]
		public static T WithTransaction<T>(this T operation, IMiddlewareOperation middleware) where T : IMiddlewareOperation
		{
			//if (operation is MiddlewareOperation o)
			//	o.AttachTransaction(middleware);

			return operation;
		}

		internal static void SetContext(this IMiddlewareObject target, IMiddlewareContext context)
		{
			ReflectionExtensions.SetPropertyValue(target, nameof(target.Context), context);
		}
	}
}
