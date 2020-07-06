using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.TextServices.Services;

namespace TomPIT.Ide.TextServices.CSharp.Services
{
	internal class DefinitionProviderService : CSharpEditorService, IDefinitionProviderService
	{
		public DefinitionProviderService(CSharpEditor editor) : base(editor)
		{
		}

		public ILocation ProvideDefinition(IPosition position)
		{
			var caret = Editor.Document.GetCaret(position);
			var model = Editor.Document.GetSemanticModelAsync().Result;
			var nodeToken = model.SyntaxTree.GetRoot().FindToken(caret);
			var symbol = model.GetSymbolInfo(nodeToken.Parent);

			if (symbol.Symbol != null)
			{
				if (symbol.Symbol.DeclaringSyntaxReferences.Length == 0)
					return null;

				var refs = symbol.Symbol.DeclaringSyntaxReferences[0];

				if (string.IsNullOrWhiteSpace(refs.SyntaxTree.FilePath))
					return null;

				var line = Editor.Document.GetLine(refs.Span.Start);
				var length = refs.Span.End - refs.Span.Start;
				var start = refs.Span.Start - line.Start + 1;
				var end = start + length;

				return new Languages.Location
				{
					Range = new Range
					{
						EndColumn = end,
						EndLineNumber = line.LineNumber + 1,
						StartColumn = start,
						StartLineNumber = line.LineNumber + 1
					},
					Uri = ResolveModel(refs.SyntaxTree.FilePath)
				};
			}

			return null;
		}

		private string ResolveModel(string path)
		{
			var result = Editor.Context.Tenant.GetService<ICompilerService>().ResolveText(Editor.Context.MicroService.Token, path);

			if (result == null)
				return null;

			var component = Editor.Context.Tenant.GetService<IComponentService>().SelectComponent(result.Configuration().Component);
			var ms = Editor.Context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

			return $"inmemory://{ms.Name}/{component.Category}/{component.Name}/{result.Id.ToString()}";
		}

	}
}
