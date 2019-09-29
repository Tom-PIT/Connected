using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Ide.TextServices.Services;

namespace TomPIT.Ide.TextServices.CSharp.Services
{
	internal class SyntaxCheckService : CSharpEditorService, ISyntaxCheckService
	{
		public SyntaxCheckService(CSharpEditor editor) : base(editor)
		{
		}

		public List<IMarkerData> CheckSyntax(ISourceCode sourceCode)
		{
			IScriptDescriptor script = null;
			var service = Editor.Context.Tenant.GetService<ICompilerService>();

			if (Editor.HostType == null)
				script = service.GetScript(sourceCode.Configuration().MicroService(), sourceCode);
			else
			{
				var methods = service.GetType().GetMethods().Where(f => string.Compare(f.Name, nameof(ICompilerService.GetScript), false) == 0);

				foreach (var method in methods)
				{
					if (method.IsGenericMethod)
					{
						var target = method.MakeGenericMethod(new Type[] { Editor.HostType });

						script = target.Invoke(service, new object[] { sourceCode.Configuration().MicroService(), sourceCode }) as IScriptDescriptor;
						break;
					}
				}
			}

			if (script == null)
				return new List<IMarkerData>();

			var result = new List<IMarkerData>();

			var ms = Editor.Context.Tenant.GetService<IMicroServiceService>().Select(sourceCode.Configuration().MicroService());
			var scriptName = sourceCode.ScriptName(Editor.Context.Tenant);
			var fileName = $"{ms.Name}/{scriptName}";

			foreach (var diagnostic in script.Errors)
			{
				if (diagnostic.Source == null)
					continue;

				if (diagnostic.Source.Contains("/"))
				{
					if (string.Compare(fileName, diagnostic.Source, true) != 0)
						continue;
				}
				else
				{
					if (string.Compare(scriptName, diagnostic.Source, true) != 0)
						continue;
				}

				var marker = new MarkerData
				{
					EndColumn = diagnostic.EndColumn + 1,
					EndLineNumber = diagnostic.EndLine + 1,
					Message = diagnostic.Message,
					Severity = diagnostic.Severity.ToMarkerSeverity(),
					Source = diagnostic.Source,
					Code = diagnostic.Code,
					StartColumn = diagnostic.StartColumn + 1,
					StartLineNumber = diagnostic.StartLine + 1
				};

				result.Add(marker);

			}

			return result;

		}
	}
}