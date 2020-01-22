using System.Collections.Generic;

namespace TomPIT.IoC
{
	public interface IDependencyInjectionService
	{
		List<IDependencyInjectionObject> QueryApiDependencies(string api, object arguments);
	}
}
