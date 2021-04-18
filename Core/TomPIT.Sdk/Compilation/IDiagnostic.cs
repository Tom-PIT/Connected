using Microsoft.CodeAnalysis;

namespace TomPIT.Compilation
{
	public interface IDiagnostic
	{
		string Message { get; }
		DiagnosticSeverity Severity { get; }

		int StartLine { get; }
		int EndLine { get; }
		int StartColumn { get; }
		int EndColumn { get; }
		string Code { get; }
		string Source { get; }
		string SourcePath { get; }
	}
}
