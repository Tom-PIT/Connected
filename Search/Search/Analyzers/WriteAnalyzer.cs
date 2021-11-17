using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using TomPIT.Search.Filters;
using TomPIT.Search.Tokenizers;

namespace TomPIT.Search.Analyzers
{
	internal class WriteAnalyzer : Analyzer
	{
		public override TokenStream TokenStream(string fieldName, TextReader reader)
		{
			var content = string.Empty;

			if (string.Compare(fieldName, "content", true) == 0)
			{
				content = reader.ReadToEnd();
				reader = new StringReader(content);
			}

			var tokenStream = new AlphaNumericTokenizer(reader);
			TokenStream result = new StandardFilter(tokenStream);

			if (!SearchUtils.IsStaticField(fieldName))
				result = new LengthFilter(result, 1, 255);

			result = new LowercaseFilter(result);
			result = new StopFilter(true, result, ReadAnalyzer.StopWords);

			if (!string.IsNullOrWhiteSpace(content))
				result = new LocalizedFilter(result, content);

			return result;
		}
	}
}