using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Analysis.Diagnostics
{
	public interface ICodeDiagnosticProvider
	{
		List<IDiagnosticInfo> CheckSyntax(IEnvironment environment, IText sourceCode, Type argumentType);
	}
}
