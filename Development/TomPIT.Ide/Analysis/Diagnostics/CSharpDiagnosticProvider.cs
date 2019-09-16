using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Analysis.Diagnostics
{
	internal class CSharpDiagnosticProvider : ICodeDiagnosticProvider
	{
		public List<IDiagnosticInfo> CheckSyntax(IEnvironment environment, ISourceCode sourceCode, Type argumentType)
		{
			var svc = environment.Context.Tenant.GetService<ICodeAnalysisService>();
			var mi = argumentType == null
				 ? svc.GetType().GetMethod("CheckSyntax", 0, new Type[] { typeof(Guid), typeof(ISourceCode) })
				 : svc.GetType().GetMethod("CheckSyntax", 1, new Type[] { typeof(Guid), typeof(ISourceCode) });

			if (argumentType != null)
				mi = mi.MakeGenericMethod(argumentType);

			var errors = (List<IDiagnostic>)mi.Invoke(svc, new object[] { environment.Context.MicroService.Token, sourceCode });

			var r = new List<IDiagnosticInfo>();

			environment.Context.Tenant.GetService<IDesignerService>().ClearErrors(sourceCode.Configuration().Component, sourceCode.Id, ErrorCategory.Syntax);

			if (errors == null || errors.Count == 0)
				return r;

			var errorList = new List<IDevelopmentError>();

			foreach (var i in errors)
			{
				r.Add(new DiagnosticInfo(i));

				if (IsSuppressed(i))
					continue;

				errorList.Add(new DevelopmentError
				{
					Element = sourceCode.Id,
					Message = i.Message,
					Severity = (DevelopmentSeverity)(int)i.Severity,
					Category = ErrorCategory.Syntax
				});
			}

			if (errorList.Count > 0)
				environment.Context.Tenant.GetService<IDesignerService>().InsertErrors(sourceCode.Configuration().Component, errorList);

			return r;
		}

		private bool IsSuppressed(IDiagnostic item)
		{
			if (item.Severity == DiagnosticSeverity.Warning && string.Compare(item.Id, "CS1702", true) == 0)
				return true;

			return false;
		}
	}
}
