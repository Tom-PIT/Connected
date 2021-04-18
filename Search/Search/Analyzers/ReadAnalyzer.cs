using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using TomPIT.Search.Filters;
using TomPIT.Search.Tokenizers;

namespace TomPIT.Search.Analyzers
{
	internal enum AnalyzerContext
	{
		Term = 1,
		Query = 2
	}
	internal class ReadAnalyzer : Analyzer
	{
		internal static readonly CharArraySet StopWords = null;

		public ReadAnalyzer(AnalyzerContext context)
		{
			Context = context;
		}

		private AnalyzerContext Context { get; } = AnalyzerContext.Term;
		static ReadAnalyzer()
		{
			StopWords = new CharArraySet(8, true)
			{
				"the",
				"www",
				"http",
				"https",
				"com"
			};
		}

		public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			var tokenStream = new AlphaNumericTokenizer(reader);
			TokenStream result = new StandardFilter(tokenStream);

			if (!SearchUtils.IsStaticField(fieldName) && Context == AnalyzerContext.Term)
				result = new LengthFilter(result, 2, 255);

			result = new LowercaseFilter(result);
			result = new StopFilter(true, result, StopWords);

			return result;
		}
	}
}