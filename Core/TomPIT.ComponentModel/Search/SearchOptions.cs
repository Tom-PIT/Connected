using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Services;

namespace TomPIT.Search
{
	public class SearchOptions : ISearchOptions
	{
		private HighlightOptions _highlight = null;
		private GlobalizationOptions _globalization = null;
		private PagingOptions _paging = null;
		private List<string> _catalogs = null;
		public string CommandText {get;set;}
		public QueryKind Kind { get; set; } = QueryKind.Term;

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

		public List<string> Catalogs
		{
			get
			{
				if (_catalogs == null)
					_catalogs = new List<string>();

				return _catalogs;
			}
		}

		public void AddCatalog([CodeAnalysisProvider(ExecutionContext.SearchCatalogProvider)]string catalog)
		{
			Catalogs.Add(catalog);
		}
	}
}
