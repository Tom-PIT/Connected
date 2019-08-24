using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomPIT.Ide.CodeAnalysis
{
	public static class CodeAnalysisExtensions
	{
		public static MemberDeclarationSyntax DeclarationScope(this SyntaxNode node)
		{
			var current = node;

			while (current != null)
			{
				if (current is MemberDeclarationSyntax md)
					return md;

				current = current.Parent;
			}

			return null;
		}

		public static VariableDeclaratorSyntax VariableDeclaration(this MemberDeclarationSyntax declaration, string variableName)
		{
			var declarations = declaration.DescendantNodes().OfType<VariableDeclarationSyntax>();

			foreach (var dc in declarations)
			{
				var target = dc.Variables.FirstOrDefault(f => string.Compare(variableName, f.Identifier.Text, false) == 0);

				if (target != null)
					return target;
			}

			return null;
		}

		public static string RenderValue(Type type)
		{
			if (type == typeof(string))
				return $"string.Empty";
			else if (type == typeof(int))
				return "0";
			else if (type == typeof(Guid))
				return "Guid.Empty";
			else if (type == typeof(DateTime))
				return "DateTime.MinValue";
			else
				return "null";
		}
	}
}