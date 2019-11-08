using System;
using System.Collections.Generic;
using TomPIT.Search;

namespace TomPIT.Ide.Search
{
	public interface IIdeSearchService
	{
		void Delete(Guid component);
		void Delete(Guid component, Guid element);

		List<ISysSearchResult> Search(ISearchOptions options);
	}
}
