using System.Collections.Generic;

namespace TomPIT.Search
{
	public interface ISearchResultsContainer
	{
		List<ISearchResultMessage> Messages { get; }

		int Total { get; }
		int SearchTime { get; }
	}
}
