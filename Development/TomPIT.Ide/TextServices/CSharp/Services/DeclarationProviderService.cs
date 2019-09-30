using TomPIT.Ide.Analysis;
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
			var enclosingIdentifier = CSharpQuery.EnclosingIdentifier(nodeToken);
			var scope = enclosingIdentifier.Parent.DeclarationScope();

			//var declaration = model.GetDeclaredSymbol(node);
			return new Languages.Location
			{
				Range = new Range
				{
					EndColumn = 10,
					EndLineNumber = 1,
					StartColumn = 1,
					StartLineNumber = 1
				},
				Uri = Editor.Model.Uri
			};
		}
	}
}
