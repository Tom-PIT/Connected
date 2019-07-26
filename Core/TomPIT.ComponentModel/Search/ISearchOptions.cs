using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public enum QueryKind
	{
		NotSet = 0,
		Term = 1,
		Query = 2
	}

	public interface ISearchOptions
	{
		string CommandText { get; set; }
		QueryKind Kind { get; set; }
		ISearchHighlightOptions Highlight { get; }
		ISearchGlobalizationOptions Globalization { get; }
		ISearchPagingOptions Paging { get; }
		ISearchResultsOptions Results { get; }
		ISearchParserOptions Parser { get; }

		List<string> Catalogs { get; }
	}
}
