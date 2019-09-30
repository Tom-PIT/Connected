using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Ide.TextServices.CSharp;
using TomPIT.Ide.TextServices.Services;

namespace TomPIT.Ide.TextServices.Razor.Services
{
	internal class SyntaxCheckService : RazorEditorService, ISyntaxCheckService
	{
		public SyntaxCheckService(RazorEditor editor) : base(editor)
		{
		}

		public List<IMarkerData> CheckSyntax(IText sourceCode)
		{
			var model = Editor.Document.GetSemanticModelAsync().Result;
			var compilation = model.Compilation;

			var result = new List<IMarkerData>();
			var diagnostics = compilation.GetDiagnostics();
			foreach (var diagnostic in diagnostics)
			{
				var source = diagnostic.Location.SourceTree?.FilePath;

				if (string.IsNullOrWhiteSpace(source))
					continue;

				if (string.Compare(Editor.Document.Name, source, true) != 0)
					continue;

				var location = diagnostic.Location.GetMappedLineSpan();

				var marker = new MarkerData
				{
					EndColumn = location.EndLinePosition.Character + 1,
					EndLineNumber = location.EndLinePosition.Line + 1,
					Message = diagnostic.GetMessage(),
					Severity = diagnostic.Severity.ToMarkerSeverity(),
					Source = source,
					Code = diagnostic.Id,
					StartColumn = location.StartLinePosition.Character + 1,
					StartLineNumber = location.StartLinePosition.Line + 1
				};

				result.Add(marker);

			}

			return result;
		}
	}
}
