using System;

namespace TomPIT.ComponentModel
{
	public interface IMicroServiceResolutionService
	{
		IMicroService ResolveMicroService(object instance);
		IMicroService ResolveMicroService(Type type);
	}
}
