namespace TomPIT.Ide.TextServices.Languages
{
	public class TextEdit : ITextEdit
	{
		public EndOfLineSequence Eol { get; set; } = EndOfLineSequence.CRLF;

		public IRange Range { get; set; }

		public string Text { get; set; }
	}
}
