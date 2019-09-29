namespace TomPIT.Ide.TextServices
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
