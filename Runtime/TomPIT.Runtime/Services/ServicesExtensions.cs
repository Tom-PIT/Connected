using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Data;

namespace TomPIT.Services
{
	public static class ServicesExtensions
	{
		public static T CreateProcessHandler<T>(this ISysConnection connection, Guid microService, Type handlerType) where T : class, IProcessHandler
		{
			var instance = handlerType.CreateInstance<T>();

			if (instance == null)
				return null;

			var ms = connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var ctx = ExecutionContext.Create(connection.Url, ms);
			var dataCtx = new DataModelContext(ctx);

			instance.Initialize(dataCtx);

			return instance;
		}
	}
}
