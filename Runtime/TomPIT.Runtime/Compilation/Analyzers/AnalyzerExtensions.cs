using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomPIT.Compilation.Analyzers
{
	internal static class AnalyzerExtensions
	{
		public static bool IsPublic(this MemberDeclarationSyntax node)
		{
			foreach(var modifier in node.Modifiers)
			{
				if (modifier.IsKind(SyntaxKind.PublicKeyword))
					return true;
			}

			return false;
		}
	}
}
