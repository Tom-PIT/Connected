using System.Collections.Generic;

namespace TomPIT.IoC
{
	public interface IUIDependencyInjectionService
	{
		List<IUIDependencyDescriptor> QueryViewDependencies(string view, object arguments);
		List<IUIDependencyDescriptor> QueryPartialDependencies(string partial, object arguments);
	}
}
