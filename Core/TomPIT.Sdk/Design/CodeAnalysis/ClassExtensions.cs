using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomPIT.Design.CodeAnalysis
{
	public static class ClassExtensions
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

		public static bool IsPartial(this ClassDeclarationSyntax declaration)
		{
			return declaration.Modifiers.Any(f => f.IsKind(SyntaxKind.PartialKeyword));
		}

		public static bool IsPublic(this ClassDeclarationSyntax declaration)
		{
			return declaration.Modifiers.Any(f => f.IsKind(SyntaxKind.PublicKeyword));
		}

		public static bool IsPlatformClass(string name)
		{
			return string.Compare(name, "__ScriptInfo", false) == 0;
		}

		public static string ClassName(this ClassDeclarationSyntax syntax, SemanticModel model)
		{
			return syntax.Identifier.ToString();
		}
	}
}
