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
				if (symbol.Symbol.Locations.Length == 0 || !symbol.Symbol.Locations[0].IsInSource)
					return null;

				var span = symbol.Symbol.Locations[0].GetLineSpan();

				return new Languages.Location
				{
					Range = new Range
					{
						EndColumn = span.EndLinePosition.Character,
						EndLineNumber = span.EndLinePosition.Line,
						StartColumn = span.StartLinePosition.Character,
						StartLineNumber = span.StartLinePosition.Line
					},
					Uri = ResolveModel(symbol.Symbol.DeclaringSyntaxReferences[0].SyntaxTree.FilePath)
				};
			}

			return null;
		}

		private string ResolveModel(string path)
		{
			var result = string.IsNullOrWhiteSpace(path)
				? Editor.Script
				: Editor.Context.Tenant.GetService<ICompilerService>().ResolveText(Editor.Context.MicroService.Token, path);

			if (result == null)
				return null;

			var component = Editor.Context.Tenant.GetService<IComponentService>().SelectComponent(result.Configuration().Component);
			var ms = Editor.Context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

			return $"inmemory://{ms.Name}/{component.Category}/{component.Name}/{result.Id.ToString()}";
		}

	}
}
