using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace TomPIT.Design.Services
{
	internal class DiagnosticInfo : IDiagnosticInfo
	{
		public const int SeverityInfo = 2;
		public const int SeverityError = 8;
		public const int SeverityHint = 1;
		public const int SeverityWarning = 4;

		public DiagnosticInfo(Diagnostic diagnostic)
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

		private void Parse(Diagnostic diagnostic)
		{
			var span = diagnostic.Location.GetMappedLineSpan();

			StartLineNumber = span.StartLinePosition.Line+1;
			StartColumn = span.StartLinePosition.Character + 1;
			EndLineNumber = span.EndLinePosition.Line+1;
			EndColumn = span.EndLinePosition.Character + 1;
			Message = diagnostic.GetMessage();

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
