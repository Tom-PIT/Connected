using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Reflection;

namespace TomPIT.Compilation;
public static class SyntaxBrowser
{
	public static ImmutableArray<TypeDeclarationSyntax> QueryDeclaredTypes(IDocumentIdentity id)
	{
		var tree = GetSyntaxTree(id);

		if (tree is null)
			return ImmutableArray<TypeDeclarationSyntax>.Empty;

		var walker = new TypeDeclarationWalker();

		walker.Visit(tree.GetRoot());

		return walker.Items.ToImmutableArray();
	}

	public static ImmutableArray<PropertyDeclarationSyntax> QueryProperties(string filePath)
	{
		var text = Tenant.GetService<IDiscoveryService>().Configuration.Find(filePath);

		if (text is null)
			return ImmutableArray<PropertyDeclarationSyntax>.Empty;

		var id = DocumentIdentity.From(text);
		var typeName = filePath.Split('/')[^1];

		var tree = GetSyntaxTree(id);

		if (tree is null)
			return ImmutableArray<PropertyDeclarationSyntax>.Empty;

		var walker = new PropertyDeclarationWalker(typeName);

		walker.Visit(tree.GetRoot());

		return walker.Items.ToImmutableArray();
	}

	private static CSharpSyntaxTree GetSyntaxTree(IDocumentIdentity id)
	{
		var text = Tenant.GetService<IDiscoveryService>().Configuration.Find(id.Component, id.Element) as IText;

		if (text is null)
			return null;

		var sourceCode = Tenant.GetService<IComponentService>().SelectText(id.MicroService, text);

		if (string.IsNullOrWhiteSpace(sourceCode))
			return null;

		sourceCode = Tenant.GetService<ICompilerService>().Rewrite(sourceCode);

		return CSharpSyntaxTree.ParseText(sourceCode, new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None, SourceCodeKind.Script)) as CSharpSyntaxTree;
	}
}
