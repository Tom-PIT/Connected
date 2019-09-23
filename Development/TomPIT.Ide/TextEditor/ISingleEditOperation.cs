namespace TomPIT.Ide.TextEditor
{
	public interface ISingleEditOperation
	{
		bool ForceMoveMakers { get; }
		IRange Range { get; set; }
		string Text { get; }
	}
}
