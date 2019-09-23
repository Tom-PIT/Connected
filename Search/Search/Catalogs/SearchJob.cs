using System.Collections.Generic;
using System.Web;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.Search;
using TomPIT.Serialization;

namespace TomPIT.Search.Catalogs
{
	internal abstract class SearchJob
	{
		private List<string> _duplicates = null;

		protected SearchJob(ISearchCatalogConfiguration catalog, ISearchOptions options)
		{
			Catalog = catalog;
			Options = options;
		}
		public CatalogSearcher Searcher { get; set; }
		public SearchResultDocuments Results { get; private set; }
		public int Total { get; private set; }
		public string CommandText { get; private set; }
		protected ISearchOptions Options { get; }

		protected abstract string ParseCommandText();

		protected abstract string PrepareCommandText(string commandText);
		protected abstract MultiFieldQueryParser CreateParser();

		public void Search(CatalogSearcher searcher)
		{
			Searcher = searcher;
			Results = new SearchResultDocuments(Options);

			if (string.IsNullOrWhiteSpace(Options.CommandText))
				return;

			CommandText = PrepareCommandText(HttpUtility.HtmlDecode(Options.CommandText));

			searcher.Reader.IncRef();

			try
			{
				var calculatedMax = Options.Paging.Size;

				if (Options.Paging.Index > 0)
					calculatedMax = Options.Paging.Size + (Options.Paging.Size * Options.Paging.Size);

				var collector = TopScoreDocCollector.Create(calculatedMax, true);
				var parser = CreateParser();

				parser.AllowLeadingWildcard = Options.Parser.AllowLeadingWildcard;

				var query = parser.Parse(ParseCommandText());

				searcher.Searcher.Search(query, collector);

				var hits = collector.TopDocs().ScoreDocs;

				Total = collector.TotalHits;

				Results.Query = query;
				Results.Analyzer = parser.Analyzer;

				var start = 0;

				if (Options.Paging.Index >= 0)
					start = Options.Paging.Index * Options.Paging.Size;

				for (var hit = start; hit < collector.TotalHits; hit++)
				{
					if (hits.Length <= hit)
						break;

					var sd = hits[hit];
					var doc = searcher.Searcher.Doc(sd.Doc);
					var searchResult = new SearchResult
					{
						Score = sd.Score
					};

					var content = new JObject();
					var fields = doc.GetFields();

					foreach (var field in fields)
					{
						if (string.Compare(field.Name, SearchUtils.FieldTitle, true) == 0)
							searchResult.Title = field.StringValue;
						else if (string.Compare(field.Name, SearchUtils.FieldText, true) == 0)
							searchResult.Text = field.StringValue;

						content.Add(field.Name, field.StringValue);
					}

					searchResult.Content = SerializationExtensions.Serialize(content);
					searchResult.Catalog = Catalog.Component;

					Results.Add(searchResult);
				}
			}
			finally
			{
				searcher.Reader.DecRef();
			}
		}

		protected ISearchCatalogConfiguration Catalog { get; }
	}
}