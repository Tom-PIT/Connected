namespace TomPIT.Ide.TextEditor
{
	public interface IRelatedInformation
	{
		int EndColumn { get; }
		int EndLineNumber { get; }
		string Message { get; }
		string Uri { get; }
		int StartColumn { get; }
		int StartLineNumber { get; }
	}
}
