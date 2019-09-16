using Microsoft.CodeAnalysis;

namespace TomPIT.Compilation
{
	internal class Diagnostic : IDiagnostic
	{
		public string Message { get; set; }
		public DiagnosticSeverity Severity { get; set; }
		public int StartLine { get; set; }
		public int EndLine { get; set; }
		public int StartColumn { get; set; }
		public int EndColumn { get; set; }
		public string Id { get; set; }
	}
}
