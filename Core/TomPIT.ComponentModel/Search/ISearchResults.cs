using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public interface ISearchResults
	{
		List<ISearchResultDescriptor> Items { get; }
		List<ISearchResultMessage> Messages { get; }

		int Total { get; }
		int SearchTime { get; }
	}
}
