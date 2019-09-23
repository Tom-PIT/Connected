using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomPIT.Ide.TextEditor.CSharp
{
	public static class CSharpAnalysisExtensions
	{
		public static TypeInfo ResolveMemberAccessTypeInfo(SemanticModel model, SyntaxNode node)
		{
			var member = node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();

			if (member == null)
				return default;

			if (member.Expression is IdentifierNameSyntax identifier)
				return model.GetTypeInfo(identifier);
			else if (member.Expression is MemberAccessExpressionSyntax memberExpression)
				return ResolveMemberAccessTypeInfo(model, memberExpression);

			return default;
		}

		public static TypeInfo ResolveTypeInfo(SemanticModel model, SyntaxNode node)
		{
			var type = model.GetSpeculativeTypeInfo(node.Span.Start, node, SpeculativeBindingOption.BindAsTypeOrNamespace);

			if (type.Type != null)
				return type;

			if (node is BaseTypeSyntax bts)
				return model.GetSpeculativeTypeInfo(node.Span.Start, bts.Type, SpeculativeBindingOption.BindAsTypeOrNamespace);

			return default;
		}

		public static SyntaxToken EnclosingIdentifier(SyntaxToken token)
		{
			var currenToken = token;

			while (true)
			{
				if (!currenToken.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.IdentifierToken)
					&& !currenToken.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DotToken))
					break;

				currenToken = currenToken.GetPreviousToken();
			}

			if (currenToken.GetNextToken().IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.IdentifierToken))
				return currenToken.GetNextToken();

			return default;
		}
	}
}
