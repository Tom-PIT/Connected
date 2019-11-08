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
	}
}
