using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TomPIT.Ide.TextEditor
{
	public static class TextEditorExtensions
	{
		public static TextSpan GetSpan(this Document document, IRange range)
		{
			var text = document.GetTextAsync().Result;
			var span = text.Lines[range.StartLineNumber - 1].Span;

			return TextSpan.FromBounds(span.Start + range.StartColumn - 1, span.Start + range.StartColumn - 1);
		}

		public static TextLine GetLine(this Document document, int position)
		{
			var text = document.GetTextAsync().Result;

			return TextLine.FromSpan(text, TextSpan.FromBounds(position, position));
		}

		public static int GetPosition(this Document document, IRange range)
		{
			var text = document.GetTextAsync().Result;
			var span = text.Lines[range.StartLineNumber - 1].Span;

			return span.Start + range.StartColumn - 1;
		}
	}
}
