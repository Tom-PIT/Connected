using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Services;

namespace TomPIT.Ide.CodeAnalysis
{
	public static class CodeAnalysisDataUtils
	{
		public static string ResolveCommandText(this VariableDeclaratorSyntax syntax, IExecutionContext context)
		{
			var invocations = syntax.DescendantNodes().OfType<InvocationExpressionSyntax>();

			if (invocations.Count() == 0)
				return null;

			var invocation = invocations.First();

			if (invocation.ArgumentList.Arguments.Count < 2)
				return null;

			if (invocation.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax le)
				return le.Token.ValueText;
			else
				return null;
		}

		public static IConnection ResolveConnection(this VariableDeclaratorSyntax syntax, IExecutionContext context)
		{
			var invocations = syntax.DescendantNodes().OfType<InvocationExpressionSyntax>();

			if (invocations.Count() == 0)
				return null;

			var invocation = invocations.First();

			if (invocation.ArgumentList.Arguments.Count == 0)
				return null;

			if (invocation.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax le)
				return le.ResolveConnection(context);
			else if (invocation.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax identifier)
				return identifier.ResolveConnection(context);
			else
				return null;
		}

		public static IConnection ResolveConnection(this IdentifierNameSyntax ins, IExecutionContext context)
		{
			var identifierName = ins.Identifier.Text;

			if (string.IsNullOrWhiteSpace(identifierName))
				return null;

			var declaration = ins.DeclarationScope();

			if (declaration == null)
				return null;

			var variable = declaration.VariableDeclaration(identifierName);

			if (variable == null)
				return null;

			return variable.ResolveConnection(context);
		}

		public static IConnection ResolveConnection(this LiteralExpressionSyntax le, IExecutionContext context)
		{
			var text = le.Token.ValueText;

			if (string.IsNullOrWhiteSpace(text))
				return null;

			var tokens = text.Split('/');

			if (tokens.Length != 2)
				return null;

			var ms = context.Connection().GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				return null;

			return context.Connection().GetService<IComponentService>().SelectConfiguration(ms.Token, "Connection", tokens[1]) as IConnection;
		}

		public static ISchemaBrowser ResolveSchemaBrowser(this IConnection connection, IExecutionContext context)
		{
			if (connection == null || connection.DataProvider == Guid.Empty)
				return null;

			var dataProvider = context.Connection().GetService<IDataProviderService>().Select(connection.DataProvider);

			if (dataProvider == null)
				return null;

			var att = dataProvider.GetType().FindAttribute<SchemaBrowserAttribute>();

			if (att == null)
				return null;

			return att.Type == null
				? Types.GetType(att.TypeName).CreateInstance<ISchemaBrowser>()
				: att.Type.CreateInstance<ISchemaBrowser>();
		}

		public static IConnection DefaultConnection(this IExecutionContext context)
		{
			var connections = context.Connection().GetService<IComponentService>().QueryComponents(context.MicroService.Token, ComponentCategories.Connection);

			if (connections != null && connections.Count == 1)
				return context.Connection().GetService<IComponentService>().SelectConfiguration(connections[0].Token) as IConnection;

			return null;
		}
	}
}
