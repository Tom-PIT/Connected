using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Development.ComponentModel
{
	public interface IDependencyChain
	{
		List<IElement> QueryDependencies();
	}
}
