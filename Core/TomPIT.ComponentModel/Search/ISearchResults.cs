using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public interface ISearchResults: ISearchResultsContainer
	{
		List<ISearchResult> Items { get; }
	}
}
