using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel.Data;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class CommandTextProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var node = Arguments.Model.SyntaxTree.GetRoot().FindNode(Editor.GetMappedSpan(Arguments.Position));

			if (node == null)
				return default;

			var parent = node.Parent as ArgumentListSyntax;
			var argument = parent.Arguments[0];

			IConnectionConfiguration connection = null;

			if (argument.Expression is LiteralExpressionSyntax le)
				connection = le.ResolveConnection(Editor.Context);
			else if (argument.Expression is IdentifierNameSyntax ins)
				connection = ins.ResolveConnection(Editor.Context);

			if (connection == null)
				connection = Editor.Context.DefaultConnection();

			if (connection == null)
				return null;

			return ConnectionItems(connection);
		}

		private List<ICompletionItem> ConnectionItems(IConnectionConfiguration connection)
		{
			var r = new List<ICompletionItem>();
			var browser = connection.ResolveSchemaBrowser(Editor.Context);

			if (browser == null)
				return null;

			var objects = browser.QueryGroupObjects(connection);

			if (objects == null)
				return r;

			foreach (var o in objects)
			{
				r.Add(new CompletionItem
				{
					Detail = o.Description,
					FilterText = o.Text,
					InsertText = o.Value,
					Kind = CompletionItemKind.Text,
					Label = o.Text,
					SortText = o.Text
				});
			}

			return r;
		}
	}
}
