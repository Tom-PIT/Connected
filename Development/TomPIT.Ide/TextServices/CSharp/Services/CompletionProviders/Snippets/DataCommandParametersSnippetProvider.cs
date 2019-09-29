using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders.Snippets
{
	internal class DataCommandParametersSnippetProvider : SnippetProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(Editor.Document);
			var span = service.GetDefaultCompletionListSpan(Editor.SourceText, Editor.Document.GetPosition(Arguments.Position));
			var node = Arguments.Model.SyntaxTree.GetRoot().FindNode(span);

			if (node == null)
				return default;

			var variableName = ResolveVariableName(node);

			if (string.IsNullOrEmpty(variableName))
				return default;

			var declaration = node.DeclarationScope();

			if (declaration == null)
				return default;

			var variable = declaration.VariableDeclaration(variableName);

			if (variable == null)
				return default;

			var connection = variable.ResolveConnection(Editor.Context);

			if (connection == null)
				connection = Editor.Context.DefaultConnection();

			if (connection == null)
				return default;

			var commandText = variable.ResolveCommandText(Editor.Context);

			if (string.IsNullOrWhiteSpace(commandText))
				return default;

			var schemaBrowser = connection.ResolveSchemaBrowser(Editor.Context);

			if (schemaBrowser == null)
				return default;

			var parameters = schemaBrowser.QueryParameters(connection, commandText);

			if (parameters == null || parameters.Count == 0)
				return default;

			var result = new List<ICompletionItem>();
			var insertText = new StringBuilder();
			var isFirst = true;

			foreach (var parameter in parameters)
			{
				if (!isFirst)
					insertText.Append($"{variableName}.");

				var mapping = parameter.IsNullable ? ", true" : string.Empty;

				insertText.AppendLine($"SetParameter(\"{parameter.Name}\", {CodeAnalysisExtensions.RenderValue(Types.ToType(parameter.DataType))}{mapping});");

				isFirst = false;
			}

			result.Add(new CompletionItem
			{
				Label = "BindParameters",
				InsertText = insertText.ToString(),
				SortText = "BindParameters",
				FilterText = "BindParameters",
				Kind = CompletionItemKind.Snippet,
				Preselect = true
			});

			return result;
		}

		private string ResolveVariableName(SyntaxNode node)
		{
			if (node is ExpressionStatementSyntax statement)
			{
				if (statement.Expression is MemberAccessExpressionSyntax member)
				{
					if (member.Expression is IdentifierNameSyntax identifier)
						return identifier.Identifier.ValueText;
				}
			}
			else if (node is QualifiedNameSyntax qn)
			{
				if (qn.Left is IdentifierNameSyntax ins)
					return ins.Identifier.ValueText;
			}
			else if (node is MemberAccessExpressionSyntax ma)
			{
				if (ma.Expression is IdentifierNameSyntax identifier)
					return identifier.Identifier.ValueText;
			}

			return null;
		}
	}
}
