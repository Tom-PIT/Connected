using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Data;

namespace TomPIT.Services
{
	public static class ServicesExtensions
	{
		public static T CreateProcessHandler<T>(this ISysConnection connection, ISourceCode sourceCode, string typeName, string arguments = null) where T : class, IProcessHandler
		{
			var ms = sourceCode.Configuration().MicroService(connection);
			var type = connection.GetService<ICompilerService>().ResolveType(ms, sourceCode, typeName);

			return CreateProcessHandler<T>(connection, ms, type, arguments);
		}

		public static T CreateProcessHandler<T>(this ISysConnection connection, ISourceCode sourceCode, string arguments = null) where T : class, IProcessHandler
		{
			var ms = sourceCode.Configuration().MicroService(connection);
			var type = connection.GetService<ICompilerService>().ResolveType(ms, sourceCode, sourceCode.Configuration().ComponentName(connection));

			return CreateProcessHandler<T>(connection, ms, type, arguments);
		}
		public static T CreateProcessHandler<T>(this ISysConnection connection, Guid microService, Type handlerType) where T : class, IProcessHandler
		{
			return CreateProcessHandler<T>(connection, microService, handlerType, null);
		}

		public static T CreateProcessHandler<T>(this ISysConnection connection, Guid microService, Type handlerType, string arguments) where T : class, IProcessHandler
		{
			var instance = handlerType.CreateInstance<T>();

			if (instance == null)
				return null;

			var ms = connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var ctx = ExecutionContext.Create(connection.Url, ms);
			var dataCtx = new DataModelContext(ctx);

			if (arguments != null)
				Types.Populate(arguments, instance);

			instance.Initialize(dataCtx);

			return instance;
		}
	}
}
