using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.DefinitionProviders
{
	internal class DefaultDefinitionProvider : DefinitionProvider
	{
		protected override ILocation OnProvideDefinition(DefinitionProviderArgs e)
		{
			var caret = e.Editor.Document.GetCaret(e.Position);
			var nodeToken = e.Model.SyntaxTree.GetRoot().FindToken(caret);
			var symbol = e.Model.GetSymbolInfo(nodeToken.Parent);

			if (symbol.Symbol == null)
				return null;

			if (symbol.Symbol.Locations.Length == 0 || !symbol.Symbol.Locations[0].IsInSource)
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
				Uri = e.ResolveModel(symbol.Symbol.DeclaringSyntaxReferences[0].SyntaxTree.FilePath)
			};
		}
	}
}
