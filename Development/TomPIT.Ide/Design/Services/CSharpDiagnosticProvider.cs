using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Ide;
using TomPIT.Ide.Design;

namespace TomPIT.Design.Services
{
	internal class CSharpDiagnosticProvider : ICodeDiagnosticProvider
	{
		public List<IDiagnosticInfo> CheckSyntax(IEnvironment environment, ISourceCode sourceCode, Type argumentType)
		{
			var svc = environment.Context.Connection().GetService<ICodeAnalysisService>();
			var mi = argumentType == null
				 ? svc.GetType().GetMethod("CheckSyntax", 0, new Type[] { typeof(Guid), typeof(ISourceCode) })
				 : svc.GetType().GetMethod("CheckSyntax", 1, new Type[] { typeof(Guid), typeof(ISourceCode) });

			if (argumentType != null)
				mi = mi.MakeGenericMethod(argumentType);

			var errors = (ImmutableArray<Diagnostic>)mi.Invoke(svc, new object[] { environment.Context.MicroService.Token, sourceCode });

			var r = new List<IDiagnosticInfo>();
			environment.Context.Connection().GetService<IDesignerService>().ClearErrors(sourceCode.Configuration().Component, sourceCode.Id);

			if (errors.IsDefaultOrEmpty)
				return r;

			var errorList = new List<IDevelopmentComponentError>();

			foreach (var i in errors)
			{
				r.Add(new DiagnosticInfo(i));

				if (IsSuppressed(i))
					continue;

				errorList.Add(new DevelopmentComponentError
				{
					Element = sourceCode.Id,
					Message = i.GetMessage(),
					Severity = (DevelopmentSeverity)(int)i.Severity
				});
			}

			if (errorList.Count > 0)
				environment.Context.Connection().GetService<IDesignerService>().InsertErrors(sourceCode.Configuration().Component, errorList);

			return r;
		}

		private bool IsSuppressed(Diagnostic item)
		{
			if (item.Severity == DiagnosticSeverity.Warning && string.Compare(item.Id, "CS1702", true) == 0)
				return true;

			return false;
		}
	}
}
