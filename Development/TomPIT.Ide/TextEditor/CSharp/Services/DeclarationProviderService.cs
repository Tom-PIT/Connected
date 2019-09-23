using TomPIT.Ide.Analysis;
using TomPIT.Ide.TextEditor.Services;
using ILocation = TomPIT.Ide.TextEditor.Languages.ILocation;

namespace TomPIT.Ide.TextEditor.CSharp.Services
{
	internal class DeclarationProviderService : CSharpEditorService, IDeclarationProviderService
	{
		public DeclarationProviderService(CSharpEditor editor) : base(editor)
		{
		}

		public ILocation ProvideDeclaration(IPosition position)
		{
			var caret = Editor.Document.GetPosition(position);
			var model = Editor.Document.GetSemanticModelAsync().Result;
			var nodeToken = model.SyntaxTree.GetRoot().FindToken(caret);
			var enclosingIdentifier = CSharpAnalysisExtensions.EnclosingIdentifier(nodeToken);
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
