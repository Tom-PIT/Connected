using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Ide;

namespace TomPIT.Design.Services
{
	internal class CSharpDiagnosticProvider : ICodeDiagnosticProvider
	{
		public List<IDiagnosticInfo> CheckSyntax(IEnvironment environment, ISourceCode sourceCode, Type argumentType)
		{
			var svc = environment.Context.Connection().GetService<ICodeAnalysisService>();
            var mi = argumentType == null
                ? svc.GetType().GetMethod("CheckSyntax", 0, new Type[] { typeof(Guid), typeof(ISourceCode)})
                : svc.GetType().GetMethod("CheckSyntax", 1, new Type[] { typeof(Guid), typeof(ISourceCode) });

            if (argumentType != null)
                mi = mi.MakeGenericMethod(argumentType);

            var errors = (ImmutableArray<Diagnostic>)mi.Invoke(svc, new object[] { environment.Context.MicroService.Token, sourceCode });
            var r = new List<IDiagnosticInfo>();

			foreach (var i in errors)
				r.Add(new DiagnosticInfo(i));

			return r;
		}
	}
}
