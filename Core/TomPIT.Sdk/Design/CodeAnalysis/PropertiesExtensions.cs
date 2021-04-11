using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomPIT.Design.CodeAnalysis
{
	public static class PropertiesExtensions
	{
		public static PropertyDeclarationSyntax FindProperty(this SyntaxNode scope, string name)
		{
			var declarations = scope.DescendantNodes().OfType<PropertyDeclarationSyntax>();

			if (declarations == null)
				return null;

			return declarations.FirstOrDefault(f => string.Compare(f.Identifier.ValueText, name, false) == 0);
		}

		public static IPropertySymbol GetPropertySymbol(this AssignmentExpressionSyntax syntax, SemanticModel model)
		{
			var symbol = model.GetSymbolInfo(syntax.Left);

			if (symbol.Symbol is IPropertySymbol property)
				return property;

			return null;
		}
	}
}
