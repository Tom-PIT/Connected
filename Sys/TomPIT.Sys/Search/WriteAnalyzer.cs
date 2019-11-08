using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace TomPIT.Sys.Search
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

			result = new LowercaseFilter(result);
			result = new StopFilter(true, result, ReadAnalyzer.StopWords);

			return result;
		}
	}
}