using System;
using Microsoft.AspNetCore.Http;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Runtime
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

		//public static ServerUrl CreateUrl(this ITenant tenant, string baseUrl, string microService, string api, string operation)
		//{
		//	return new ApiUrl(baseUrl, microService, api, operation);
		//}

		public static bool IsMicroServiceSupported(this ITenant tenant, Guid microService)
		{
			var ms = tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return false;

			return Instance.ResourceGroupExists(ms.ResourceGroup);
		}

		public static IMicroServiceContext CreateContext(this IComponent component)
		{
			return new MicroServiceContext(component.MicroService, MiddlewareDescriptor.Current.Tenant.Url);
		}

		public static IMicroServiceContext CreateContext(this IConfiguration configuration)
		{
			return CreateContext(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(configuration.Component));
		}

		public static void SetContext(this IMiddlewareObject target, IMiddlewareContext context)
		{
			ReflectionExtensions.SetPropertyValue(target, nameof(target.Context), context);
		}
	}
}