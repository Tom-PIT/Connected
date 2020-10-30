using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TomPIT.Ide.TextServices
{
	public static class TextEditorExtensions
	{
		public static TextSpan GetSpan(this SourceText text, IPosition position)
		{
			var span = text.Lines[position.LineNumber].Span;

			return TextSpan.FromBounds(span.Start + position.Column, span.Start + position.Column);
		}

		public static TextSpan GetSpan(this Document document, IPosition position)
		{
			var text = document.GetTextAsync().Result;

			if (text.Lines.Count < position.LineNumber)
				return default;

			var span = text.Lines[position.LineNumber].Span;

			return TextSpan.FromBounds(span.Start + position.Column, span.Start + position.Column);
		}

		public static TextSpan GetSpan(this Document document, IRange range)
		{
			var text = document.GetTextAsync().Result;

			if (text.Lines.Count < range.StartLineNumber || text.Lines.Count < range.EndLineNumber)
				return default;

			var spanStart = text.Lines[range.StartLineNumber].Span;
			var spanEnd = text.Lines[range.EndLineNumber].Span;

			return TextSpan.FromBounds(spanStart.Start + range.StartColumn, spanEnd.Start + range.EndColumn);
		}

		public static TextLine GetLine(this SourceText text, int position)
		{
			foreach (var line in text.Lines)
			{
				if (line.Span.IntersectsWith(position))
					return line;
			}

			return new TextLine();
		}

		public static TextLine GetLine(string sourceCode, int position)
		{
			var text = SourceText.From(sourceCode);

			foreach (var line in text.Lines)
			{
				if (line.Span.IntersectsWith(position))
					return line;
			}

			return new TextLine();
		}

		public static TextLine GetLine(this Document document, int position)
		{
			var text = document.GetTextAsync().Result;

			foreach (var line in text.Lines)
			{
				if (line.Span.IntersectsWith(position))
					return line;
			}

			return new TextLine();
		}

		public static int GetCaret(this SourceText text, LinePosition position)
		{
			var span = text.Lines[position.Line].Span;

			return span.Start + position.Character;
		}

		public static int GetCaret(this SourceText text, IPosition position)
		{
			var span = text.Lines[position.LineNumber].Span;

			return span.Start + position.Column;
		}

		public static int GetCaret(this Document document, IRange range)
		{
			var text = document.GetTextAsync().Result;
			var span = text.Lines[range.StartLineNumber].Span;

			return span.Start + range.StartColumn;
		}

		public static int GetCaret(this Document document, LinePosition position)
		{
			var text = document.GetTextAsync().Result;
			var span = text.Lines[position.Line].Span;

			return span.Start + position.Character;
		}

		public static int GetCaret(this Document document, IPosition position)
		{
			var text = document.GetTextAsync().Result;
			var span = text.Lines[position.LineNumber].Span;

			return span.Start + position.Column;
		}
	}
}
