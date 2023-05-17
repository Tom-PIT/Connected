using System;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class SearchController : ISearchController
	{
		public void Index(Guid microService, string catalog, string arguments)
		{
			DataModel.Search.Insert(microService, catalog, arguments);
		}
	}
}
