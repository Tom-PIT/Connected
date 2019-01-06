namespace TomPIT.Design.Services
{
	public interface IDiagnosticInfo
	{
		int StartLineNumber { get; }
		int StartColumn { get; }
		int EndLineNumber { get; }
		int EndColumn { get; }
		string Message { get; }
		int Severity { get; }
	}
}
