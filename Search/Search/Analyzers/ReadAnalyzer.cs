using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using TomPIT.Search.Filters;
using TomPIT.Search.Tokenizers;

namespace TomPIT.Search.Analyzers
{
	internal class ReadAnalyzer : Analyzer
	{
		internal static readonly CharArraySet StopWords = null;

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

			if (!SearchUtils.IsStaticField(fieldName))
				result = new LengthFilter(result, 2, 255);

			result = new LowercaseFilter(result);
			result = new StopFilter(true, result, StopWords);

			return result;
		}
	}
}