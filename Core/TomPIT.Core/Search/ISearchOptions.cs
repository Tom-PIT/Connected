namespace TomPIT.Search
{
	public enum CommandKind
	{
		NotSet = 0,
		Term = 1,
		Query = 2
	}

	public interface ISearchOptions
	{
		string CommandText { get; set; }
		CommandKind Kind { get; set; }
		ISearchHighlightOptions Highlight { get; }
		ISearchGlobalizationOptions Globalization { get; }
		ISearchPagingOptions Paging { get; }
		ISearchResultsOptions Results { get; }
		ISearchParserOptions Parser { get; }
	}
}
