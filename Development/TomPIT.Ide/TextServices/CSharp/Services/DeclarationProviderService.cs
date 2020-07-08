using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TomPIT.Ide.TextServices.Services;
using ILocation = TomPIT.Ide.TextServices.Languages.ILocation;

namespace TomPIT.Ide.TextServices.CSharp.Services
{
	internal class DeclarationProviderService : CSharpEditorService, IDeclarationProviderService
	{
		public DeclarationProviderService(CSharpEditor editor) : base(editor)
		{
		}

		public ILocation ProvideDeclaration(IPosition position)
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

				if (!string.IsNullOrWhiteSpace(refs.SyntaxTree.FilePath))
					return null;

				var span = symbol.Symbol.Locations[0].GetLineSpan();

				return new Languages.Location
				{
					Range = new Range
					{
						EndColumn = span.EndLinePosition.Character + 1,
						EndLineNumber = span.EndLinePosition.Line + 1,
						StartColumn = span.StartLinePosition.Character + 1,
						StartLineNumber = span.StartLinePosition.Line + 1
					},
					Uri = Editor.Model.Uri
				};
			}

			return null;
		}
	}
}