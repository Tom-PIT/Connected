using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Search;

namespace TomPIT.SysDb.Search
{
	public interface ISearchHandler
	{
		void Insert(IMicroService microService, string catalog, Guid identifier, DateTime created, string arguments);
		List<IIndexRequest> Query();
		IIndexRequest Select(Guid identifier);
		void Delete(IIndexRequest d);

		ICatalogState SelectState(Guid catalog);
		void UpdateState(Guid catalog, CatalogStateStatus status);
		void InvalidateState(Guid catalog);
		void DeleteState(Guid catalog);
	}
}
