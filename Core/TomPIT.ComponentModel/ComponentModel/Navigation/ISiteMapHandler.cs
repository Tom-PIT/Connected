using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.Navigation
{
	public interface ISiteMapHandler : IProcessHandler
	{
		List<ISiteMapContainer> Invoke(params string[] key);
	}
}
