using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.Data;
using TomPIT.Search.Indexing;

namespace TomPIT.Search.Catalogs
{
	internal class SearchTransaction
	{
		public SearchResults Results { get; private set; }
		private ISearchOptions Options { get; set; }
		public void Search(ISearchOptions options)
		{
			Options = options;
			Results = new SearchResults();

			Search();
		}

		private void Search()
		{
			if (Options.Catalogs == null || Options.Catalogs.Count == 0)
				return;

			var r = new SearchResultDocuments(Options);
			var sw = new Stopwatch();

			sw.Start();

			Parallel.ForEach(Options.Catalogs, f =>
			{
				try
				{
					var catalogTokens = f.Split("/".ToCharArray(), 2);
					var ms = Instance.GetService<IMicroServiceService>().Select(catalogTokens[0]);
					var config = Instance.GetService<IComponentService>().SelectConfiguration(ms.Token, "SearchCatalog", catalogTokens[1]);
					var catalog = IndexCache.Ensure(config.Component);

					if (catalog == null || !catalog.IsValid)
						return;

					var sr = Search(catalog);

					if (sr != null && sr.Count > 0)
					{
						r.Analyzer = sr.Analyzer;
						r.Query = sr.Query;
						r.AddRange(sr);
					}
				}
				catch(Exception ex)
				{
					Results.Messages.Add(new SearchResultMessage
					{
						Type = SearchResultMessageType.Error,
						Text = ex.Message
					});
				}
			});

			sw.Stop();

			Results.SearchTime = (int)sw.ElapsedMilliseconds;

			if (r.Count > 0)
			{
				var results = r.OrderByDescending(f => f.Score).ThenBy(f => f.Title).Take(Options.Paging.Size);

				foreach (SearchResultDescriptor result in results)
					Results.Items.Add(result);
			}

			foreach (SearchResultDescriptor i in Results.Items)
				new SearchTextParser(r, i, Options).Parse();
		}

		private SearchResultDocuments Search(CatalogHost catalog)
		{
			try
			{
				var result = catalog.Search(Options);

				Results.Total += result.Item2;

				return result.Item1;
			}
			catch (Exception ex)
			{
				Results.Messages.Add(new SearchResultMessage
				{
					Type = SearchResultMessageType.Error,
					Text = ex.Message
				});

				return null;
			}
		}
	}
}