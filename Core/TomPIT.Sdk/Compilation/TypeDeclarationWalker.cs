using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace TomPIT.Compilation;
internal class TypeDeclarationWalker : CSharpSyntaxWalker
{
	public TypeDeclarationWalker()
	{
		Items = new();
	}
	public List<TypeDeclarationSyntax> Items { get; }
	public override void Visit(SyntaxNode? node)
	{
		base.Visit(node);
	}

	public override void VisitClassDeclaration(ClassDeclarationSyntax node)
	{
		Items.Add(node);
		base.VisitClassDeclaration(node);
	}
}
