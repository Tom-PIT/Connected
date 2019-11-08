namespace TomPIT.Ide.TextServices
{
	public interface ISingleEditOperation
	{
		bool ForceMoveMakers { get; }
		IRange Range { get; set; }
		string Text { get; }
	}
}
