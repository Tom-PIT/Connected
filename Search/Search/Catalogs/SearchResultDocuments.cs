using Lucene.Net.Analysis;
using Lucene.Net.Search.Highlight;
using System.Collections.Generic;
using System.IO;

namespace TomPIT.Search.Catalogs
{
	public class SearchResultDocuments : List<SearchResult>
	{
		private IFormatter _formatter = null;
		private SimpleFragmenter _fragmenter = null;
		private Highlighter _highlighter = null;
		private QueryScorer _scorer = null;
		private readonly object sync = new object();

		public SearchResultDocuments(ISearchOptions options)
		{
			Options = options;

			if (!string.IsNullOrWhiteSpace(Options.Highlight.PreTag))
				PreTag = Options.Highlight.PreTag;

			if (!string.IsNullOrWhiteSpace(Options.Highlight.PostTag))
				PostTag = Options.Highlight.PostTag;
		}

		private ISearchOptions Options { get; }
		private string PreTag { get; } = "<span style=\"font-weight:bold;\">";
		private string PostTag { get; } = "</span>";
		public void Append(List<SearchResult> ds)
		{
			if (ds == null || ds.Count == 0)
				return;

			lock (sync)
			{
				AddRange(ds);
			}
		}

		internal Lucene.Net.Search.Query Query { get; set; }
		internal Analyzer Analyzer { get; set; }

		public string Highlight(string text, int length)
		{
			if (_formatter == null)
			{
				_formatter = new SimpleHTMLFormatter(PreTag, PostTag);
				_fragmenter = new SimpleFragmenter(length);
				_scorer = new QueryScorer(Query);
				_highlighter = new Highlighter(_formatter, _scorer);
				_highlighter.TextFragmenter = _fragmenter;
			}

			using (var sr = new StringReader(text))
			{
				var stream = Analyzer.TokenStream(string.Empty, sr);

				return _highlighter.GetBestFragments(stream, text, 2, "...");
			}
		}
	}
}