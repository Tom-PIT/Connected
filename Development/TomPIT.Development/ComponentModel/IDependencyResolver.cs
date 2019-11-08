using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Development.ComponentModel
{
	public interface IDependencyResolver
	{
		List<IDependency> Resolve(IComponent component);
	}
}