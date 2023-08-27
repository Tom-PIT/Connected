using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace TomPIT.Compilation;

internal class NamespaceRewriter : CSharpSyntaxRewriter
{
    public static string Rewrite(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var rewriter = new NamespaceRewriter();
        var tree = CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose, SourceCodeKind.Script));
        var newTree = rewriter.Visit(tree.GetRoot()).SyntaxTree;

        return newTree.GetRoot().WithLeadingTrivia(tree.GetRoot().GetLeadingTrivia()).SyntaxTree.ToString();
    }

    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        var tokens = node.Name.ToFullString().Split('.');

        return CreatePartial(node, tokens);
    }

    private ClassDeclarationSyntax CreatePartial(FileScopedNamespaceDeclarationSyntax node, IEnumerable<string> tokens)
    {
        var members = new List<MemberDeclarationSyntax>();

        if (tokens.Count() == 1)
        {
            foreach (var child in node.ChildNodes())
            {
                if (child is MemberDeclarationSyntax declaration)
                    members.Add(WithLineNumber(declaration) as MemberDeclarationSyntax);

            }
        }
        else
            members.Add(CreatePartial(node, tokens.Skip(1)));

        var result = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(tokens.ElementAt(0)));

        result = result.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        result = result.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        result = result.NormalizeWhitespace();
        result = result.AddMembers(members.ToArray());

        return result;
    }

    private static SyntaxNode WithLineNumber(SyntaxNode node)
    {
        var line = node.GetLocation().GetLineSpan();

        return node.WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($"#line {line.StartLinePosition.Line + 1}").Add(SyntaxFactory.EndOfLine("\n")));
    }
}
