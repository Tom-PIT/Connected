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
		public static IConnectionString ResolveConnectionString(this IConnectionConfiguration connection, IMiddlewareContext context, ConnectionStringContext connectionContext)
		{
			return ResolveConnectionString(connection, context, connectionContext, null);
		}
		public static IConnectionString ResolveConnectionString(this IConnectionConfiguration connection, IMiddlewareContext context, ConnectionStringContext connectionContext, object arguments)
		{
			var ms = connection.MicroService();
			var componentName = connection.ComponentName();
			var type = context.Tenant.GetService<ICompilerService>().ResolveType(ms, connection, componentName, false);

			if (type == null)
				return StaticConnectionConfiguration(connection);

			var e = arguments == null ? string.Empty : Serializer.Serialize(arguments);
			var middleware = context.Tenant.GetService<ICompilerService>().CreateInstance<ConnectionMiddleware>(new MicroServiceContext(ms, context), type, e);

			var result = middleware.Invoke(new ConnectionMiddlewareArgs(connectionContext));

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
