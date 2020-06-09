using System;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Data
{
	public static class DataExtensions
	{
		public static IConnectionString ResolveConnectionString(this IConnectionConfiguration connection, IMiddlewareContext context)
		{
			return ResolveConnectionString(connection, context, null);
		}
		public static IConnectionString ResolveConnectionString(this IConnectionConfiguration connection, IMiddlewareContext context, object arguments)
		{
			var ms = connection.MicroService();
			var componentName = connection.ComponentName();
			var type = context.Tenant.GetService<ICompilerService>().ResolveType(ms, connection, componentName, false);

			if (type == null)
				return StaticConnectionConfiguration(connection);

			var e = arguments == null ? string.Empty : Serializer.Serialize(arguments);
			var result = context.Tenant.GetService<ICompilerService>().CreateInstance<ConnectionMiddleware>(new MicroServiceContext(ms, context), type, e).Invoke();

			if (result == null)
				return StaticConnectionConfiguration(connection);

			if (result.DataProvider == Guid.Empty && connection.DataProvider != Guid.Empty && result is ConnectionString cs)
				cs.DataProvider = connection.DataProvider;

			return result;
		}

		private static IConnectionString StaticConnectionConfiguration(IConnectionConfiguration configuration)
		{
			return new ConnectionString
			{
				DataProvider = configuration.DataProvider,
				Value = configuration.Value
			};
		}
	}
}
