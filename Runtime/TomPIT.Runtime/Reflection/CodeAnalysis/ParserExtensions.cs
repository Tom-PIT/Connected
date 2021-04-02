using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomPIT.Reflection.CodeAnalysis
{
	internal static class ParserExtensions
	{
		public static bool IsOfType(this INamedTypeSymbol symbol, Type type)
		{
			return string.Compare(ToDisplayName(symbol), type.FullTypeName(), false) == 0;
		}

		public static bool IsOfType(this TypeInfo symbol, Type type)
		{
			if (symbol.ConvertedType == null)
				return false;

			return string.Compare(ToDisplayName(symbol.ConvertedType), type.FullTypeName(), false) == 0;
		}

		public static TypeInfo GetAttribute<T>(this SyntaxList<AttributeListSyntax> attributes, SemanticModel model)
		{
			foreach (var list in attributes)
			{
				if (list is AttributeListSyntax listSyntax)
				{
					foreach (var attribute in listSyntax.Attributes)
					{
						var typeInfo = model.GetTypeInfo(attribute);

						if (IsOfType(typeInfo, typeof(T)))
							return typeInfo;
					}
				}
			}

			return default;
		}

		public static bool ContainsAttribute<T>(this SyntaxList<AttributeListSyntax> attributes, SemanticModel model)
		{
			return attributes.GetAttribute<T>(model).ConvertedType != null;
		}
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

		public static string ParseDocumentation(this CSharpSyntaxNode node)
		{
			if (node == null)
				return null;

			var trivias = node.GetLeadingTrivia();

			if (trivias.Count == 0)
				return null;

			var enumerator = trivias.GetEnumerator();

			while (enumerator.MoveNext())
			{
				var trivia = enumerator.Current;

				if (trivia.Kind().Equals(SyntaxKind.SingleLineDocumentationCommentTrivia))
				{
					var xml = trivia.GetStructure();

					if (xml == null)
						continue;

					var fullString = xml.ToFullString();
					var sb = new StringBuilder();
					string currentLine;

					using var r = new StringReader(fullString);

					while ((currentLine = r.ReadLine()) != null)
					{
						currentLine = currentLine.Trim();

						if (currentLine.StartsWith("///"))
							currentLine = currentLine.Substring(3).Trim();

						sb.AppendLine(currentLine);
					}

					return sb.ToString();
				}
			}

			return null;
		}
	}
}
