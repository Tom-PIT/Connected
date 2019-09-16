using System;
using System.Web;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using TomPIT.Search;

namespace TomPIT.Sys.Search
{
	internal abstract class SearchJob
	{
		protected SearchJob(ISearchOptions options)
		{
			Options = options;
		}
		public Searcher Searcher { get; set; }
		public SearchResultDocuments Results { get; private set; }
		public int Total { get; private set; }
		public string CommandText { get; private set; }
		protected ISearchOptions Options { get; }

		protected abstract string ParseCommandText();

		protected abstract string PrepareCommandText(string commandText);
		protected abstract MultiFieldQueryParser CreateParser();

		public void Search(Searcher searcher)
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

				searcher.Index.Search(query, collector);

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
					var doc = searcher.Index.Doc(sd.Doc);
					var searchResult = new SysSearchResult
					{
						Score = sd.Score
					};

					var fields = doc.GetFields();

					foreach (var field in fields)
					{
						if (string.Compare(field.Name, SearchFields.Title, true) == 0)
							searchResult.Title = field.StringValue;
						else if (string.Compare(field.Name, SearchFields.Content, true) == 0)
							searchResult.Content = field.StringValue;
						else if (string.Compare(field.Name, SearchFields.Component, true) == 0)
							searchResult.Component = Guid.Parse(field.StringValue);
						else if (string.Compare(field.Name, SearchFields.ComponentName, true) == 0)
							searchResult.ComponentName = field.StringValue;
						else if (string.Compare(field.Name, SearchFields.Element, true) == 0)
							searchResult.Element = Guid.Parse(field.StringValue);
						else if (string.Compare(field.Name, SearchFields.ElementName, true) == 0)
							searchResult.ElementName = field.StringValue;
						else if (string.Compare(field.Name, SearchFields.Microservice, true) == 0)
							searchResult.MicroService = new Guid(field.StringValue);
						else if (string.Compare(field.Name, SearchFields.Tags, true) == 0)
							searchResult.Tags = field.StringValue;
					}

					Results.Add(searchResult);
				}
			}
			finally
			{
				searcher.Reader.DecRef();
			}
		}
	}
}