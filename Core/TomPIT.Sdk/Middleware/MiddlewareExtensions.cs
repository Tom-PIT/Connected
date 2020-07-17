using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware.Services;
using TomPIT.Reflection;
using TomPIT.Security;

namespace TomPIT.Middleware
{
	public static class MiddlewareExtensions
	{
		public static ServerUrl CreateUrl(this ITenant tenant, string controller, string action)
		{
			return ServerUrl.Create(tenant.Url, controller, action);
		}

		public static T WithContext<T>(this T operation, IMiddlewareContext context) where T : IMiddlewareOperation
		{
			if (operation.Context is MiddlewareContext op && context is MiddlewareContext mc)
			{
				if (op.Owner == mc)
					return operation;

				op.Owner = mc;

				if (operation is MiddlewareOperation mop && mc.Transaction != null)
					mop.Transaction = mc.Transaction;
			}

			return operation;
		}

		public static void Grant(this IMiddlewareContext context)
		{
			if (!(context is IElevationContext ctx))
				return;

			ctx.Grant();
		}

		public static void Revoke(this IMiddlewareContext context)
		{
			if (!(context is IElevationContext ctx))
				return;

			ctx.Revoke();
		}

		internal static void SetContext(this IMiddlewareObject target, IMiddlewareContext context)
		{
			ReflectionExtensions.SetPropertyValue(target, nameof(target.Context), context);
		}

		public static void Impersonate(this IMiddlewareContext context, string user)
		{
			var u = context.Services.Identity.GetUser(user);

			if (u == null)
				throw new RuntimeException(SR.ErrUserNotFound);

			if (context.Services.Identity is MiddlewareIdentityService mc)
				mc.ImpersonatedUser = u.Token.ToString();
		}

		public static void RevokeImpersonation(this IMiddlewareContext context)
		{
			if (context.Services.Identity is MiddlewareIdentityService mc)
				mc.ImpersonatedUser = null;
		}
	}
}
