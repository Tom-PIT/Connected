using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Annotations;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.TextServices;
using TomPIT.Ide.TextServices.CSharp;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Reflection;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class ExtenderCompletionProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var result = new List<ICompletionItem>();
			var span = Editor.Document.GetSpan(Arguments.Position);
			var node = Arguments.Model.SyntaxTree.GetRoot().FindNode(span);

			var objectCreation = node.Closest<ObjectCreationExpressionSyntax>();

			if (objectCreation != null)
			{
				var type = Arguments.Model.GetTypeInfo(objectCreation);

				if (type.Type == null)
					return result;

				var attributes = type.Type.FindAttributes(typeof(ExtenderAttribute).FullTypeName());

				foreach (var attribute in attributes)
				{
					if (attribute.ConstructorArguments.Length == 0)
						continue;

					var value = attribute.ConstructorArguments[0].Value;

					if (value is INamedTypeSymbol nt)
					{
						result.Add(new CompletionItem
						{
							FilterText = nt.Name,
							InsertText = nt.Name,
							Label = nt.Name,
							SortText = nt.Name,
							Kind = CompletionItemKind.Reference
						});
					}
				}
			}

			return result;
		}
	}
}
