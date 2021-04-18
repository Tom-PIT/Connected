using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Compilation;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.DefinitionProviders
{
	internal class ReferenceProvider : DefinitionProvider
	{
		protected override ILocation OnProvideDefinition(DefinitionProviderArgs e)
		{
			var caret = e.Editor.Document.GetCaret(e.Position);
			var nodeToken = e.Model.SyntaxTree.GetRoot().FindToken(caret);
			var trivia = e.Model.SyntaxTree.GetRoot().FindTrivia(caret);

			if (!trivia.IsKind(SyntaxKind.LoadDirectiveTrivia))
				return null;

			var structure = trivia.GetStructure() as LoadDirectiveTriviaSyntax;

			if (structure == null)
				return null;

			var path = e.Editor.Context.Tenant.GetService<ICompilerService>().ResolveReference(e.Editor.Context.MicroService.Token, structure.File.ValueText);

			return new Languages.Location
			{
				Range = new Range
				{
					EndColumn = 1,
					EndLineNumber = 1,
					StartColumn = 1,
					StartLineNumber = 1
				},
				Uri = e.ResolveModel(path)
			};
		}
	}
}
