using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomPIT.Ide.TextEditor.CSharp
{
	public static class CSharpAnalysisExtensions
	{
		public static TypeInfo ResolveTypeInfo(SemanticModel model, SyntaxNode node)
		{
			var member = node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();

			if (member == null)
				return new TypeInfo();

			if (!(member.Expression is IdentifierNameSyntax identifier))
				return new TypeInfo();

			return model.GetTypeInfo(identifier);
		}
	}
}
