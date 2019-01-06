using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Ide;

namespace TomPIT.Design.Services
{
	public interface ICodeDiagnosticProvider
	{
		List<IDiagnosticInfo> CheckSyntax(IEnvironment environment, ISourceCode sourceCode, Type argumentType);
	}
}
