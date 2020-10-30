using System;
using System.Collections.Generic;
using TomPIT.Search;

namespace TomPIT.Design
{
	public interface IDesignSearch
	{
		void Delete(Guid component);
		void Delete(Guid component, Guid element);

		List<ISysSearchResult> Search(ISearchOptions options);
	}
}
