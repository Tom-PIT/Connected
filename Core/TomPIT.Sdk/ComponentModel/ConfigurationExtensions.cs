using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.BigData;
using TomPIT.Compilation;
using TomPIT.ComponentModel.BigData;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.IoT;
using TomPIT.ComponentModel.Search;
using TomPIT.Data;
using TomPIT.IoT;
using TomPIT.Middleware;
using TomPIT.Search;
using TomPIT.Serialization;

namespace TomPIT.ComponentModel
{
	public static class ConfigurationExtensions
	{
		public static T WithIdentity<T>(this T context, IMiddlewareContext identityContext) where T : IMiddlewareContext
		{
			context.Impersonate(identityContext.Services.Identity.User?.Token.ToString());

			return context;
		}
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
			return GetMiddlewareType(configuration, context, typeof(IPartitionMiddleware<>));
		}

		public static Type ModelType(this IModelConfiguration configuration, IMiddlewareContext context)
		{
			return GetMiddlewareType(configuration, context, typeof(IModelMiddleware<>));
		}

		public static Type IoTHubSchemaType(this IIoTHubConfiguration configuration, IMiddlewareContext context)
		{
			return GetMiddlewareType(configuration, context, typeof(IIoTHubMiddleware<>));
		}

		public static List<PropertyInfo> GetMiddlewareProperties(Type type, bool includePrivate)
		{
			return includePrivate
				? type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList()
				: type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
		}
		private static Type GetMiddlewareType(this IText configuration, IMiddlewareContext context, Type middlewareType)
		{
			var type = context.Tenant.GetService<ICompilerService>().ResolveType(configuration.Configuration().MicroService(), configuration, configuration.Configuration().ComponentName());

			if (type == null)
				return null;

			var handler = type.GetInterface(middlewareType.FullName);

			return handler.GetGenericArguments()[0];
		}

		public static Type CatalogType(this ISearchCatalogConfiguration catalog)
		{
			var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(((IConfiguration)catalog).MicroService(), catalog, catalog.ComponentName());

			if (type == null)
				return null;

			var searchHandler = type.GetInterface(typeof(ISearchMiddleware<>).FullName);

			if (searchHandler == null)
				return null;

			return searchHandler.GetGenericArguments()[0];
		}
	}
}
