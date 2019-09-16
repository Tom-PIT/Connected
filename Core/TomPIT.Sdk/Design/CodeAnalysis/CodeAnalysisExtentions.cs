using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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

		public static ClassDeclarationSyntax FindClass(this SyntaxNode scope, string name)
		{
			var declarations = scope.DescendantNodes().OfType<ClassDeclarationSyntax>();

			if (declarations == null)
				return null;

			return declarations.FirstOrDefault(f => string.Compare(f.Identifier.ValueText, name, false) == 0);
		}

		public static bool Inherits(this ClassDeclarationSyntax scope, string baseTypeName)
		{
			if (scope.BaseList == null)
				return false;

			foreach(var baseType in scope.BaseList.Types)
			{
				if (!(baseType.Type is IdentifierNameSyntax id))
					continue;

				var tokens = id.Identifier.ValueText.Split('.');

				if (string.Compare(tokens[tokens.Length - 1], baseTypeName, false) == 0)
					return true;
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
	}
}
