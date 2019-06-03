using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using TomPIT.Compilation;

namespace TomPIT.Compilers
{
	internal class Diagnostic : IDiagnostic
	{
		public string Message {get;set;}
		public DiagnosticSeverity Severity {get;set;}
	}
}
