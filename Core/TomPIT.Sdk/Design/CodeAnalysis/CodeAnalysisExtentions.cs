using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomPIT.Design.CodeAnalysis
{
	public static class CodeAnalysisExtentions
	{
		public static string ToDisplayName(this ITypeSymbol symbol)
		{
			var sb = new StringBuilder();
			var targetSymbol = symbol;

			if (symbol is IArrayTypeSymbol array)
				targetSymbol = array.ElementType;

			if (!string.IsNullOrWhiteSpace(targetSymbol?.ContainingNamespace?.Name))
				sb.Append($"{targetSymbol.ContainingNamespace.ToDisplayString()}.");

			sb.Append(targetSymbol.MetadataName);

			if (symbol is IArrayTypeSymbol)
				sb.Append("[]");

			if (!string.IsNullOrWhiteSpace(targetSymbol?.ContainingAssembly?.Name))
				sb.Append($", {targetSymbol.ContainingAssembly.ToDisplayString()}");

			return sb.ToString();
		}

		internal static string ToManifestTypeName(ITypeSymbol symbol)
		{
			if (symbol is INamedTypeSymbol namedType)
				return namedType.ToDisplayString();

			return symbol.ToDisplayName();
		}

		internal static string ToManifestTypeName(TypeSyntax syntax)
		{
			if (syntax is PredefinedTypeSyntax predefined)
				return predefined.Keyword.ValueText;
			else
				return syntax.GetText().ToString();
		}

		public static string DeclaringTypeName(this ISymbol symbol)
		{
			if (symbol.ContainingType is null || symbol.ContainingType.ContainingNamespace is null || symbol.ContainingType.ContainingAssembly is null)
				return null;

			return $"{symbol.ContainingType.ContainingNamespace}.{symbol.ContainingType.MetadataName}, {symbol.ContainingAssembly.ToDisplayString()}";
		}

		public static bool TryResolveValue(this ExpressionSyntax syntax, SemanticModel model, out object value)
		{
			value = null;

			if (syntax is LiteralExpressionSyntax literal)
			{
				value = literal.ToString();

				return true;
			}
			else if (syntax is IdentifierNameSyntax identifier)
			{
				var symbol = model.GetSymbolInfo(identifier);

				if (symbol.Symbol is null)
					return false;
				//TODO: resolve other scenarios
				if (symbol.Symbol is IFieldSymbol field)
				{
					if (field.HasConstantValue)
					{
						value = field.ConstantValue;
						return true;
					}
				}

			}

			return false;
		}
	}
}
