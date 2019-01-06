using System.Collections.Generic;

namespace TomPIT.ComponentModel
{
	public interface IDependencyResolver
	{
		List<IDependency> Resolve(IComponent component);
	}
}