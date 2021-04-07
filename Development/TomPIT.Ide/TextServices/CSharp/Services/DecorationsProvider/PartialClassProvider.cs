using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.DecorationsProvider
{
	internal class PartialClassProvider : DeltaDecorationProvider
	{
		protected override List<IDeltaDecoration> OnProvideDecorations()
		{
			var result = new List<IDeltaDecoration>();

			var nodes = Arguments.Model.SyntaxTree.GetRoot().DescendantNodesAndSelf();

			foreach (var member in nodes)
				ProcessMember(member, result);

			return result;
		}

		private void ProcessMember(SyntaxNode node, List<IDeltaDecoration> items)
		{
			if (node is not ClassDeclarationSyntax declaration || !declaration.IsPartial())
				return;

			var line = node.GetLocation().GetLineSpan();

			items.AddRange(new List<IDeltaDecoration>
			{
				new DeltaDecoration
				{
					Range = new Range
					{
						StartLineNumber = line.StartLinePosition.Line + 1,
						StartColumn = 1,
						EndLineNumber = line.StartLinePosition.Line + 2,
						EndColumn = 1,
					},
					Options = new DeltaDecorationOptions
					{
						IsWholeLine = true,
						InlineClassNameAffectsLetterSpacing = true,
						InlineClassName = "tp-namespace",
						HoverMessage = SR.NamespaceClass
					}
				},
				new DeltaDecoration
				{
					Range = new Range
					{
						StartLineNumber = line.EndLinePosition.Line + 1,
						StartColumn = 1,
						EndLineNumber = line.EndLinePosition.Line + 1,
						EndColumn = 1,
					},
					Options = new DeltaDecorationOptions
					{
						IsWholeLine = true,
						InlineClassNameAffectsLetterSpacing = true,
						InlineClassName = "tp-namespace",
						HoverMessage = SR.NamespaceClass
					}
				}
			});
		}
	}
}