using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public interface IClientSearchResults : ISearchResultsContainer
	{
		List<IClientSearchResult> Items { get; }
	}
}