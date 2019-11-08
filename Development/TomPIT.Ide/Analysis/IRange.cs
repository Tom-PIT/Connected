namespace TomPIT.Ide.Analysis
{
	public interface IRange
	{
		int EndColumn { get; }
		int EndLineNumber { get; }
		int StartColumn { get; }
		int StartLineNumber { get; }
	}
}
