using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Search;

namespace TomPIT.Search.Catalogs
{
	internal abstract class SearchJob
	{
		private List<string> _duplicates = null;

		protected SearchJob(ISearchCatalog catalog, ISearchOptions options)
		{
			Catalog = catalog;
			Options = options;
		}
		public CatalogSearcher Searcher { get; set; }
		public SearchResultDocuments Results { get; private set; }
		public int Total { get; private set; }
		public string CommandText { get; private set; }
		protected ISearchOptions Options { get; }
		public List<string> Duplicates
		{
			get
			{
				if (_duplicates == null)
					_duplicates = new List<string>();

				return _duplicates;
			}
		}

		private void AddToResults(SearchResult r)
		{
			var existing = Results.FirstOrDefault(f => string.Compare(f.Id, r.Id, false) == 0);

			if (existing != null)
			{
				Total--;

				if (!Duplicates.Contains(r.Id))
					Duplicates.Add(r.Id);

				return;
			}

			Results.Add(r);
		}

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
					var sr = new SearchResult
					{
						Score = sd.Score
					};

					var f = doc.GetField(SearchUtils.FieldKey);

					if (f != null)
						sr.Id = f.StringValue;

					f = doc.GetField(SearchUtils.FieldLcid);

					if (f != null)
					{
						if (int.TryParse(f.StringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out int lang))
							sr.Lcid = lang;
					}

					f = doc.GetField(SearchUtils.FieldTitle);

					if (f != null)
						sr.Title = f.StringValue;

					f = doc.GetField(SearchUtils.FieldTags);

					if (f != null)
						sr.Tags = f.StringValue;

					f = doc.GetField(SearchUtils.FieldAuthor);

					if (f != null)
						sr.User = Convert.ToString(f.StringValue, CultureInfo.InvariantCulture).AsGuid();

					f = doc.GetField(SearchUtils.FieldDate);

					if (f != null)
						sr.Date = new DateTime(Convert.ToInt64(f.StringValue, CultureInfo.InvariantCulture));

					var properties = Catalog.CatalogProperties();

					if (properties != null)
					{
						foreach (var property in properties)
						{
							var srf = ResolveResult(doc, property);

							if (srf != null)
								sr.Fields.Add(srf);
						}
					}

					AddToResults(sr);
				}
			}
			finally
			{
				searcher.Reader.DecRef();
			}
		}

		private SearchResultField ResolveResult(Document doc, PropertyInfo property)
		{
			var store = property.FindAttribute<SearchStoreAttribute>();

			if (store == null || !store.Enabled)
				return null;

			var f = doc.GetField(property.Name.ToLowerInvariant());

			if (f == null)
				return null;

			var r = new SearchResultField
			{
				Name = property.Name.ToLowerInvariant()
			};

			var dt = Types.ToDataType(property.PropertyType);

			switch (dt)
			{
				case DataType.String:
					r.Value = f.StringValue;
					break;
				case DataType.Float:
				case DataType.Integer:
				case DataType.Long:
					var num = f.StringValue;

					if (!string.IsNullOrWhiteSpace(num) && double.TryParse(num, NumberStyles.Any, CultureInfo.InvariantCulture, out double dn))
							r.Value = dn.ToString(CultureInfo.InvariantCulture);

					break;
				case DataType.Bool:
					var byt = f.StringValue;

					if (!string.IsNullOrWhiteSpace(byt) && byte.TryParse(byt, NumberStyles.Any, CultureInfo.InvariantCulture, out byte bt))
							r.Value = bt == 0 ? false.ToString() : true.ToString();

					break;
				case DataType.Date:
					var dv = f.StringValue;

					if (!string.IsNullOrWhiteSpace(dv) && long.TryParse(dv, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
							r.Value = new DateTime(Convert.ToInt64(f.StringValue, CultureInfo.InvariantCulture)).ToString();

					break;
				default:
					break;
			}

			if (string.IsNullOrWhiteSpace(r.Value))
				return null;

			return r;
		}

		protected ISearchCatalog Catalog { get; }
	}
}