using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public interface ISearchResultsContainer
	{
		List<ISearchResultMessage> Messages { get; }

		int Total { get; }
		int SearchTime { get; }
	}
}
