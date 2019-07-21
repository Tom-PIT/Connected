using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

namespace TomPIT.Search.Filters
{
	internal class LowercaseFilter : TokenFilter
	{
		private readonly ITermAttribute _termAtt = null;

		public LowercaseFilter(TokenStream stream)
				: base(stream)
		{
			_termAtt = AddAttribute<ITermAttribute>();
		}

		public override bool IncrementToken()
		{
			if (input.IncrementToken())
			{
				var buffer = _termAtt.TermBuffer();
				var length = _termAtt.TermLength();

				for (var i = 0; i < length; i++)
					buffer[i] = char.ToLower(buffer[i]);

				return true;
			}

			return false;
		}
	}
}