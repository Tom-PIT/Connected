using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace TomPIT.Compilation
{
	public interface IDiagnostic
	{
		string Message { get; }
		DiagnosticSeverity Severity { get; }
	}
}
