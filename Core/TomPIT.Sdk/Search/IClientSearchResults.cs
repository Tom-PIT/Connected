using System.Collections.Generic;

namespace TomPIT.Search
{
	public interface IClientSearchResults : ISearchResultsContainer
	{
		List<IClientSearchResult> Items { get; }
	}
}