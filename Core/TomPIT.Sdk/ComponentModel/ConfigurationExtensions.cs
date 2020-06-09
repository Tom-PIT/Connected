using System;
using TomPIT.BigData;
using TomPIT.Compilation;
using TomPIT.ComponentModel.BigData;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.ComponentModel
{
	public static class ConfigurationExtensions
	{
		public static T CreateMiddleware<T>(this IMicroServiceContext context, Type type) where T : class
		{
			return CreateMiddleware<T>(context, type, null);
		}
		public static T CreateMiddleware<T>(this IMicroServiceContext context, Type type, object arguments) where T : class
		{
			var args = arguments == null ? string.Empty : Serializer.Serialize(arguments);

			return context.Tenant.GetService<ICompilerService>().CreateInstance<T>(context, type, args);
		}

		public static Type Middleware(this IText text, IMiddlewareContext context)
		{
			var config = text.Configuration();

			return context.Tenant.GetService<ICompilerService>().ResolveType(config.MicroService(), text, config.ComponentName(), false);
		}
		public static Type BigDataPartitionType(this IPartitionConfiguration configuration, IMiddlewareContext context)
		{
			var type = context.Tenant.GetService<ICompilerService>().ResolveType(configuration.MicroService(), configuration, configuration.ComponentName());

			if (type == null)
				return null;

			var handler = type.GetInterface(typeof(IPartitionMiddleware<>).FullName);

			return handler.GetGenericArguments()[0];
		}
	}
}
