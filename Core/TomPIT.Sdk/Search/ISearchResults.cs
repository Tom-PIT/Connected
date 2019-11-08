using System.Collections.Generic;

namespace TomPIT.Search
{
	public interface ISearchResults : ISearchResultsContainer
	{
		List<ISearchResult> Items { get; }
	}
}
