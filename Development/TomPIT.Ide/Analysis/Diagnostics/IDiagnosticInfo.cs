namespace TomPIT.Ide.Analysis.Diagnostics
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
