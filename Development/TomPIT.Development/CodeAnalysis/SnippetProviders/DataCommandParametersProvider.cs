using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TomPIT.ComponentModel.Data;
using TomPIT.Design.Services;
using TomPIT.Ide.CodeAnalysis;
using TomPIT.Services;

namespace TomPIT.Development.CodeAnalysis.SnippetProviders
{
	internal class DataCommandParametersProvider : ISnippetProvider
	{
		public List<ISuggestion> ProvideSnippets(SnippetArgs e)
		{
			var node = e.Model.SyntaxTree.GetRoot().FindNode(e.Span);

			if (node == null)
				return null;

			if (!(node is ExpressionStatementSyntax statement))
				return null;

			if (!(statement.Expression is MemberAccessExpressionSyntax member))
				return null;

			if (!(member.Expression is IdentifierNameSyntax identifier))
				return null;

			var declaration = node.DeclarationScope();

			if (declaration == null)
				return null;

			var variable = declaration.VariableDeclaration(identifier.Identifier.Text);

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
			var rvs = new List<IReturnValueParameter>();

			foreach (var parameter in parameters)
			{
				if (!isFirst)
					insertText.Append($"{identifier.Identifier.Text}.");

				if (parameter is IReturnValueParameter returnValue)
					rvs.Add(returnValue);
				else
				{
					var mapping = parameter.IsNullable ? ", true" : string.Empty;

					insertText.AppendLine($"SetParameter(\"{parameter.Name}\", {CodeAnalysisExtensions.RenderValue(Types.ToType(parameter.DataType))}{mapping});");
				}

				isFirst = false;
			}

			foreach(var returnValue in rvs)
			{
				if (!isFirst)
					insertText.Append($"{identifier.Identifier.Text}.");

				insertText.AppendLine($"SetReturnValueParameter(\"{returnValue.Name}\");");
			}

			result.Add(new Suggestion
			{
				Label="BindParameters",
				InsertText=insertText.ToString(),
				SortText="BindParameters",
				FilterText="BindParameters",
				Kind = Suggestion.Snippet
			});

			return result;
		}


	}
}
