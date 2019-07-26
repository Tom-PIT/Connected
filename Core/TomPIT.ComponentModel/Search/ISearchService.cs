using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Search;

namespace TomPIT.Search
{
	public enum SearchVerb
	{
		Add = 1,
		Update = 2,
		Remove = 3,
		Rebuild = 4,
		Drop = 5
	}

	public enum SearchMode
	{
		No = 1,
		Analyzed = 2,
		NotAnalyzed = 3,
		NotAnalyzedNoNorms = 4,
		AnalyzedNoNorms = 5
	}

	public enum SearchTermVector
	{
		No = 1,
		Yes = 2,
		WithPositions = 3,
		WithOffsets = 4,
		WithPositionsAndOffsets = 5
	}

	public interface ISearchService
	{
		void Index<T>(ISearchCatalog catalog, SearchVerb verb, T args);
		IClientSearchResults Search(ISearchOptions options);
	}
}
