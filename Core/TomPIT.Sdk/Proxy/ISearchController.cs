using System;

namespace TomPIT.Proxy
{
	public interface ISearchController
	{
		void Index(Guid microService, string catalog, string arguments);
	}
}
