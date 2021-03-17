using System;
using TomPIT.Compilation;
using TomPIT.Middleware;

namespace TomPIT.ComponentModel
{
	internal class MicroServiceResolutionService : IMicroServiceResolutionService
	{
		public IMicroService ResolveMicroService(object instance)
		{
			if (MiddlewareDescriptor.Current.Tenant != null)
				return MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveMicroService(instance);

			return null;
		}

		public IMicroService ResolveMicroService(Type type)
		{
			if (MiddlewareDescriptor.Current.Tenant != null)
				return MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveMicroService(type);

			return null;
		}
	}
}
