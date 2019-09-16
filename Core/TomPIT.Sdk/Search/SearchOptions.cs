using System.Collections.Generic;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Search
{
	public class SearchOptions : ICatalogSearchOptions
	{
		private HighlightOptions _highlight = null;
		private GlobalizationOptions _globalization = null;
		private PagingOptions _paging = null;
		private ResultsOptions _results = null;
		private ParserOptions _parser = null;
		private List<string> _catalogs = null;
		public string CommandText { get; set; }
		public CommandKind Kind { get; set; } = CommandKind.Term;

		public ISearchResultsOptions Results
		{
			get
			{
				if (_results == null)
					_results = new ResultsOptions();

				return _results;
			}
		}

		public ISearchHighlightOptions Highlight
		{
			get
			{
				if (_highlight == null)
					_highlight = new HighlightOptions();

				return _highlight;
			}
		}

		public ISearchGlobalizationOptions Globalization
		{
			get
			{
				if (_globalization == null)
					_globalization = new GlobalizationOptions();

				return _globalization;
			}
		}

		public ISearchPagingOptions Paging
		{
			get
			{
				if (_paging == null)
					_paging = new PagingOptions();

				return _paging;
			}
		}

		public ISearchParserOptions Parser
		{
			get
			{
				if (_parser == null)
					_parser = new ParserOptions();

				return _parser;
			}
		}

		public List<string> Catalogs
		{
			get
			{
				if (_catalogs == null)
					_catalogs = new List<string>();

				return _catalogs;
			}
		}

		public void AddCatalog([CAP(CAP.SearchCatalogProvider)]string catalog)
		{
			Catalogs.Add(catalog);
		}
	}
}
