using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Reflection;

namespace TomPIT.Design.CodeAnalysis
{
	public static class CodeAnalysisExtentions
	{
		public static ClassDeclarationSyntax DeclaredClass(this SyntaxNode scope)
		{
			while (scope != null)
			{
				if (scope is ClassDeclarationSyntax cs)
					return cs;

				scope = scope.Parent;
			}

			return null;
		}

		public static ClassDeclarationSyntax FindClass(this SyntaxTree tree, string name)
		{
			var declarations = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

			if (declarations == null)
				return null;

			return declarations.FirstOrDefault(f => string.Compare(f.Identifier.ValueText, name, false) == 0);
		}

		public static ClassDeclarationSyntax FindClass(this SyntaxNode scope, string name)
		{
			var declarations = scope.DescendantNodes().OfType<ClassDeclarationSyntax>();

			if (declarations == null)
				return null;

			return declarations.FirstOrDefault(f => string.Compare(f.Identifier.ValueText, name, false) == 0);
		}

		public static ITypeSymbol LookupBaseType(this ClassDeclarationSyntax scope, SemanticModel model, string baseTypeName)
		{
			if (scope.BaseList == null)
				return null;

			foreach (var baseType in scope.BaseList.Types)
			{
				var r = model.GetTypeInfo(baseType.Type).LookupBaseType(model, baseTypeName);

				if (r != null)
					return r;
			}

			return null;
		}

		public static ITypeSymbol LookupBaseType(this TypeInfo type, SemanticModel model, string baseTypeName)
		{
			if (type.Type == null)
				return null;

			return type.Type.LookupBaseType(model, baseTypeName);
		}

		public static ITypeSymbol LookupBaseType(INamedTypeSymbol type, SemanticModel model, string baseTypeName)
		{
			if (type == null)
				return null;

			var displayName = type.ToDisplayString();

			if (string.Compare(displayName, baseTypeName, false) == 0)
				return type;

			foreach (var itf in type.AllInterfaces)
			{
				displayName = itf.ToDisplayString();

				if (string.Compare(displayName, baseTypeName, false) == 0)
					return itf;
			}

			if (type.BaseType == null)
				return null;

			return LookupBaseType(type.BaseType, model, baseTypeName);
		}

		public static bool IsArray(this ITypeSymbol type, SemanticModel model)
		{
			return type.LookupBaseType(model, typeof(IEnumerable).FullTypeName()) != default;
		}
		public static ITypeSymbol LookupBaseType(this ITypeSymbol type, SemanticModel model, string baseTypeName)
		{
			if (type == null)
				return null;

			var displayName = type.ToDisplayString();

			if (string.Compare(displayName, baseTypeName, false) == 0)
				return type;

			foreach (var itf in type.AllInterfaces)
			{
				displayName = itf.ToDisplayString();

				if (string.Compare(displayName, baseTypeName, false) == 0)
					return itf;
			}

			if (type.BaseType == null)
				return null;

			return type.BaseType.LookupBaseType(model, baseTypeName);
		}
		public static bool IsInheritedFrom(this ITypeSymbol symbol, string type)
		{
			var current = symbol;

			while (current != null)
			{
				if (string.Compare(current.ToDisplayString(), type, false) == 0)
					return true;

				current = current.BaseType;
			}

			return false;
		}

		public static string SingleLineComment(this SyntaxToken token, TextSpan span, out int spanStart)
		{
			spanStart = 0;

			if (!token.HasLeadingTrivia)
				return null;

			var trivia = token.LeadingTrivia;

			if (!span.IntersectsWith(trivia.Span))
				return null;

			foreach (var line in trivia)
			{
				if (line.Span.IntersectsWith(span) && line.IsKind(SyntaxKind.SingleLineCommentTrivia))
				{
					var text = line.ToFullString();
					spanStart = line.SpanStart;

					if (text == null)
						return string.Empty;

					var comment = text.Trim();

					if (comment.StartsWith("//"))
					{
						comment = comment.Substring(2);
						spanStart += 2;
					}

					return comment;
				}
			}

			return null;
		}
		public static PropertyDeclarationSyntax FindProperty(this SyntaxNode scope, string name)
		{
			var declarations = scope.DescendantNodes().OfType<PropertyDeclarationSyntax>();

			if (declarations == null)
				return null;

			return declarations.FirstOrDefault(f => string.Compare(f.Identifier.ValueText, name, false) == 0);
		}

		internal static string ToDisplayName(this ITypeSymbol symbol)
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

		public static bool IsPartial(this ClassDeclarationSyntax declaration)
		{
			return declaration.Modifiers.Any(f => f.IsKind(SyntaxKind.PartialKeyword));
		}

		public static bool IsPublic(this ClassDeclarationSyntax declaration)
		{
			return declaration.Modifiers.Any(f => f.IsKind(SyntaxKind.PublicKeyword));
		}
	}
}
