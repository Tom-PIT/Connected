using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Ide.TextServices.CSharp;
using TomPIT.Ide.TextServices.Services;
using TomPIT.Runtime;

namespace TomPIT.Ide.TextServices.Razor.Services
{
	internal class SyntaxCheckService : RazorEditorService, ISyntaxCheckService
	{
		public SyntaxCheckService(RazorEditor editor) : base(editor)
		{
		}

		public List<IMarkerData> CheckSyntax(IText sourceCode)
		{
			var model = Editor.SemanticModel;// Editor.Document.GetSemanticModelAsync().Result;
			var compilation = model.Compilation;

			var result = new List<IMarkerData>();
			var stage = Editor.Context.Tenant.GetService<IRuntimeService>().Stage;
			ImmutableArray<Diagnostic> diagnostics;

			if (stage == EnvironmentStage.Production)
				diagnostics = compilation.GetDiagnostics();
			else
				diagnostics = compilation.WithAnalyzers(CreateAnalyzers(Editor.Context.Tenant, sourceCode)).GetAllDiagnosticsAsync().Result;

			foreach (var diagnostic in diagnostics)
			{
				var source = diagnostic.Location.SourceTree?.FilePath;

				if (string.IsNullOrWhiteSpace(source))
					continue;

				if (string.Compare(Editor.Document.Name, source, true) != 0)
					continue;

				if (IsSuppressed(diagnostic))
					continue;

				var location = diagnostic.Location.GetMappedLineSpan();

				var marker = new MarkerData
				{
					EndColumn = location.EndLinePosition.Character + 1,
					EndLineNumber = location.EndLinePosition.Line + 1,
					Message = diagnostic.GetMessage(),
					Severity = diagnostic.Severity.ToMarkerSeverity(),
					Source = $"{source}.cshtml",
					Code = diagnostic.Id,
					StartColumn = location.StartLinePosition.Character + 1,
					StartLineNumber = location.StartLinePosition.Line + 1
				};

				result.Add(marker);

			}

			return result;
		}

		private bool IsSuppressed(Diagnostic diagnostic)
		{
			if (string.Compare(diagnostic.Id, "CS0103", true) == 0
				&& diagnostic.GetMessage().Contains("The name 'section'"))
				return true;

			if(string.Compare(diagnostic.Id, "CS8019", true) == 0)
			{
				if( diagnostic.Location.SourceTree.GetRoot().FindNode(diagnostic.Location.SourceSpan) is UsingDirectiveSyntax syntax)
				{
					if(syntax.Name is IdentifierNameSyntax name)
					{
						if(string.Compare(name.ToFullString(), "TomPIT", false) == 0
							|| string.Compare(name.ToFullString(), "System", false) == 0)
							return true;
					}
					else if (syntax.Name is QualifiedNameSyntax qualified)
					{
						if (string.Compare(qualified.ToFullString(), "Microsoft.AspNetCore.Mvc.Rendering", false) == 0
							|| string.Compare(qualified.ToFullString(), "System.Linq", false) == 0)
							return true;
					}
				}
			}

			return false;
		}

		private static ImmutableArray<DiagnosticAnalyzer> CreateAnalyzers(ITenant tenant, IText text)
		{
			return tenant.GetService<ICompilerService>().GetAnalyzers(CompilerLanguage.Razor, text.Configuration().MicroService(), text.Configuration().Component, text.Id);
		}
	}
}
