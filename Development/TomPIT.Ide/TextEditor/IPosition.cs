namespace TomPIT.Ide.TextEditor
{
	public interface IPosition
	{
		int Column { get; }
		int LineNumber { get; }
	}
}
