using System;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Development;

namespace TomPIT.Reflection
{
	public class ManifestDiagnostic : IDevelopmentError
	{
		public Guid Component { get; set; }

		public Guid Element { get; set; }

		public DevelopmentSeverity Severity { get; set; }

		public string Message { get; set; }

		public string Code { get; set; }

		public ErrorCategory Category { get; set; }

		public Guid Identifier { get; set; }

		public static ManifestDiagnostic FromDiagnostic(IDiagnostic diagnostic, IElement element)
		{
			if (diagnostic == null)
				return default;

			var r = new ManifestDiagnostic
			{
				Category = Development.ErrorCategory.Syntax,
				Code = diagnostic.Code,
				Component = element.Configuration().Component,
				Element = element.Id,
				Message = diagnostic.Message
			};

			switch (diagnostic.Severity)
			{
				case Microsoft.CodeAnalysis.DiagnosticSeverity.Hidden:
					r.Severity = DevelopmentSeverity.Hidden;
					break;
				case Microsoft.CodeAnalysis.DiagnosticSeverity.Info:
					r.Severity = DevelopmentSeverity.Info;
					break;
				case Microsoft.CodeAnalysis.DiagnosticSeverity.Warning:
					r.Severity = DevelopmentSeverity.Warning;
					break;
				case Microsoft.CodeAnalysis.DiagnosticSeverity.Error:
					r.Severity = DevelopmentSeverity.Error;
					break;
			}

			return r;
		}
	}
}
