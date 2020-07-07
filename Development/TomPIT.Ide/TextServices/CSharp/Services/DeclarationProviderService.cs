using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
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

				var line = TextEditorExtensions.GetLine(SourceCode(refs.SyntaxTree.FilePath), refs.Span.Start);
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
					Uri = Editor.Model.Uri
				};
			}

			return null;
		}

		private string SourceCode(string path)
		{
			var result = Editor.Context.Tenant.GetService<ICompilerService>().ResolveText(Editor.Context.MicroService.Token, path);

			if (result == null)
				return null;

			return Editor.Context.Tenant.GetService<IComponentService>().SelectText(result.Configuration().MicroService(), result);
		}
	}
}