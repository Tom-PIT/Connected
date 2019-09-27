using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TomPIT.Ide.TextEditor
{
	public enum OffsetDirection
	{
		None = 0,
		Up = 1,
		Down = 2
	}
	public static class TextEditorExtensions
	{
		public static TextSpan GetSpan(this SourceText text, IPosition position)
		{
			var span = text.Lines[position.LineNumber - 1].Span;

			return TextSpan.FromBounds(span.Start + position.Column - 1, span.Start + position.Column - 1);
		}

		public static TextSpan GetSpan(this Document document, IPosition position)
		{
			var text = document.GetTextAsync().Result;
			var span = text.Lines[position.LineNumber - 1].Span;

			return TextSpan.FromBounds(span.Start + position.Column - 1, span.Start + position.Column - 1);
		}

		public static TextSpan GetSpan(this Document document, IRange range)
		{
			var text = document.GetTextAsync().Result;
			var span = text.Lines[range.StartLineNumber - 1].Span;

			return TextSpan.FromBounds(span.Start + range.StartColumn - 1, span.Start + range.StartColumn - 1);
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

		public static int GetPosition(this SourceText text, IPosition position, OffsetDirection direction = OffsetDirection.None)
		{
			var offset = 0;

			switch (direction)
			{
				case OffsetDirection.Up:
					offset = -1;
					break;
				case OffsetDirection.Down:
					offset = 1;
					break;
			}

			var span = text.Lines[position.LineNumber + offset].Span;

			return span.Start + position.Column + offset;
		}

		public static int GetPosition(this Document document, IRange range)
		{
			var text = document.GetTextAsync().Result;
			var span = text.Lines[range.StartLineNumber - 1].Span;

			return span.Start + range.StartColumn - 1;
		}

		public static int GetPosition(this Document document, LinePosition position)
		{
			var text = document.GetTextAsync().Result;
			var span = text.Lines[position.Line - 1].Span;

			return span.Start + position.Character - 1;
		}

		public static int GetPosition(this Document document, IPosition position)
		{
			var text = document.GetTextAsync().Result;
			var span = text.Lines[position.LineNumber - 1].Span;

			return span.Start + position.Column - 1;
		}
	}
}
