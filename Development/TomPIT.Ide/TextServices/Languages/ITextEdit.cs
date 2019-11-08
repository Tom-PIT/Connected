namespace TomPIT.Ide.TextServices.Languages
{
	public enum EndOfLineSequence
	{
		LF = 0,
		CRLF = 1
	}
	public interface ITextEdit
	{
		EndOfLineSequence Eol { get; }
		IRange Range { get; }
		string Text { get; }
	}
}
