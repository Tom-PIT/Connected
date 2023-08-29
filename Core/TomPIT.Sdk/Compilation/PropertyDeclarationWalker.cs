using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TomPIT.Compilation;
internal class PropertyDeclarationWalker : CSharpSyntaxWalker
{
	public PropertyDeclarationWalker(string typeName)
	{
		TypeName = typeName;
		Items = new();
	}

	private string TypeName { get; }
	public List<PropertyDeclarationSyntax> Items { get; }
	public override void Visit(SyntaxNode? node)
	{
		base.Visit(node);
	}

	public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
	{
		if (node.Parent is ClassDeclarationSyntax declarationSyntax)
		{
			if (!declarationSyntax.Modifiers.Any(f => f.IsKind(SyntaxKind.PartialKeyword))
				&& string.Equals(declarationSyntax.Identifier.Text, TypeName, StringComparison.Ordinal))
			{
				Items.Add(node);
				return;
			}
		}

		base.VisitPropertyDeclaration(node);
	}
}
