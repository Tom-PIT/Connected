using System.Collections.Generic;

namespace TomPIT.ComponentModel
{
	public interface IDependencyChain
	{
		List<IElement> QueryDependencies();
	}
}
