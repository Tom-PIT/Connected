using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using TomPIT.Compilation;

namespace TomPIT.Design.Services
{
	internal class DiagnosticInfo : IDiagnosticInfo
	{
		public const int SeverityInfo = 2;
		public const int SeverityError = 8;
		public const int SeverityHint = 1;
		public const int SeverityWarning = 4;

		public DiagnosticInfo(IDiagnostic diagnostic)
		{
			Parse(diagnostic);
		}

		[JsonProperty("startLineNumber")]
		public int StartLineNumber { get; set; }
		[JsonProperty("startColumn")]
		public int StartColumn { get; set; }
		[JsonProperty("endLineNumber")]
		public int EndLineNumber { get; set; }
		[JsonProperty("endColumn")]
		public int EndColumn { get; set; }
		[JsonProperty("message")]
		public string Message { get; set; }
		[JsonProperty("severity")]
		public int Severity { get; set; }

		private void Parse(IDiagnostic diagnostic)
		{
			StartLineNumber = diagnostic.StartLine+1;
			StartColumn = diagnostic.StartColumn + 1;
			EndLineNumber = diagnostic.EndLine+1;
			EndColumn = diagnostic.EndColumn + 1;
			Message = diagnostic.Message;

			switch (diagnostic.Severity)
			{
				case DiagnosticSeverity.Hidden:
					Severity = SeverityHint;
					break;
				case DiagnosticSeverity.Info:
					Severity = SeverityInfo;
					break;
				case DiagnosticSeverity.Warning:
					Severity = SeverityWarning;
					break;
				case DiagnosticSeverity.Error:
					Severity = SeverityError;
					break;
				default:
					break;
			}
		}
	}
}
