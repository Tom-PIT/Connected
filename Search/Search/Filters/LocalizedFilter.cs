using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using System.Text.RegularExpressions;

namespace TomPIT.Search.Filters
{
    internal class LocalizedFilter : TokenFilter
    {
        private static readonly Regex Regex = null;
        private CharArraySet _stopWords = null;

        private readonly ITermAttribute _termAtt = null;
        private readonly IPositionIncrementAttribute _posIncrAtt = null;

        static LocalizedFilter()
        {
            Regex = new Regex(@"\[base16(.*?)\]: ");
        }

        public LocalizedFilter(TokenStream input, string content)
            : base(input)
        {
            ParseStopWords(content);

            _termAtt = AddAttribute<ITermAttribute>();
            _posIncrAtt = AddAttribute<IPositionIncrementAttribute>();
        }

        private void ParseStopWords(string content)
        {
            _stopWords = new CharArraySet(16, true);

            var matches = Regex.Matches(content);

            if (matches == null)
                return;

            foreach (Match i in matches)
                _stopWords.Add(i.Value.Substring(1, i.Value.Length - 4));
        }

        public override bool IncrementToken()
        {
            var skippedPositions = 0;

            while (input.IncrementToken())
            {
                if (!_stopWords.Contains(_termAtt.TermBuffer(), 0, _termAtt.TermLength()))
                {
                    _posIncrAtt.PositionIncrement += skippedPositions;

                    return true;
                }

                skippedPositions += _posIncrAtt.PositionIncrement;
            }

            return false;
        }
    }
}