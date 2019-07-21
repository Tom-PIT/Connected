using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Util;

namespace TomPIT.Search.Tokenizers
{
    internal class AlphaNumericTokenizer : CharTokenizer
    {
        private ITermAttribute _term = null;

        public AlphaNumericTokenizer(TextReader input)
            : base(input)
        {
            _term = AddAttribute<ITermAttribute>();
        }

        public AlphaNumericTokenizer(AttributeSource source, TextReader input)
            : base(source, input)
        {
            _term = AddAttribute<ITermAttribute>();
        }

        public AlphaNumericTokenizer(AttributeFactory factory, TextReader input)
            : base(factory, input)
        {
            _term = AddAttribute<ITermAttribute>();
        }

        protected override bool IsTokenChar(char c)
        {
            return char.IsLetter(c) || char.IsNumber(c);
        }
    }
}