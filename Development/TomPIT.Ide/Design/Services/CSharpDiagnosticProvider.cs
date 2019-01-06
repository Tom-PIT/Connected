using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using TomPIT.Compilers;
using TomPIT.ComponentModel;
using TomPIT.Ide;

namespace TomPIT.Design.Services
{
	internal class CSharpDiagnosticProvider : ICodeDiagnosticProvider
	{
		public List<IDiagnosticInfo> CheckSyntax(IEnvironment environment, ISourceCode sourceCode, Type argumentType)
		{
			var svc = environment.Context.Connection().GetService<ICodeAnalysisService>();
			var mi = svc.GetType().GetMethod("CheckSyntax");
			var generic = mi.MakeGenericMethod(argumentType);
			var errors = (ImmutableArray<Diagnostic>)generic.Invoke(svc, new object[] { environment.Context.MicroService(), sourceCode });
			var r = new List<IDiagnosticInfo>();

			foreach (var i in errors)
				r.Add(new DiagnosticInfo(i));

			return r;
		}
	}
}
