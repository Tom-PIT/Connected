using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Design.Services;
using TomPIT.Ide.CodeAnalysis;

namespace TomPIT.Development.CodeAnalysis.SnippetProviders
{
	internal class DataCommandParametersProvider : ISnippetProvider
	{
		public List<ISuggestion> ProvideSnippets(SnippetArgs e)
		{
			var node = e.Model.SyntaxTree.GetRoot().FindNode(e.Span);

			if (node == null)
				return null;

			var variableName = ResolveVariable(node);

			if (string.IsNullOrEmpty(variableName))
				return null;

			var declaration = node.DeclarationScope();

			if (declaration == null)
				return null;

			var variable = declaration.VariableDeclaration(variableName);

			if (variable == null)
				return null;

			var connection = variable.ResolveConnection(e.Context);

			if (connection == null)
				return null;

			var commandText = variable.ResolveCommandText(e.Context);

			if (string.IsNullOrWhiteSpace(commandText))
				return null;

			var schemaBrowser = connection.ResolveSchemaBrowser(e.Context);

			if (schemaBrowser == null)
				return null;

			var parameters = schemaBrowser.QueryParameters(connection, commandText);

			if (parameters == null || parameters.Count == 0)
				return null;

			var result = new List<ISuggestion>();
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

			result.Add(new Suggestion
			{
				Label = "BindParameters",
				InsertText = insertText.ToString(),
				SortText = "BindParameters",
				FilterText = "BindParameters",
				Kind = Suggestion.Snippet
			});

			return result;
		}

		private string ResolveVariable(SyntaxNode node)
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

			return null;
		}
	}
}
