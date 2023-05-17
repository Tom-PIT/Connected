using System;

namespace TomPIT.Proxy.Remote
{
	internal class SearchController : ISearchController
	{
		private const string Controller = "Search";
		public void Index(Guid microService, string catalog, string arguments)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Index"), new
			{
				microService,
				catalog,
				arguments
			});
		}
	}
}
