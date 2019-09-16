using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.DataProviders.Design;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Ide.Analysis
{
	public static class CodeAnalysisDataUtils
	{
		public static string ResolveCommandText(this VariableDeclaratorSyntax syntax, IMiddlewareContext context)
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

		public static IConnectionConfiguration ResolveConnection(this VariableDeclaratorSyntax syntax, IMiddlewareContext context)
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

		public static IConnectionConfiguration ResolveConnection(this IdentifierNameSyntax ins, IMiddlewareContext context)
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

		public static IConnectionConfiguration ResolveConnection(this LiteralExpressionSyntax le, IMiddlewareContext context)
		{
			var text = le.Token.ValueText;

			if (string.IsNullOrWhiteSpace(text))
				return null;

			var tokens = text.Split('/');

			if (tokens.Length != 2)
				return null;

			var ms = context.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				return null;

			return context.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, ComponentCategories.Connection, tokens[1]) as IConnectionConfiguration;
		}

		public static ISchemaBrowser ResolveSchemaBrowser(this IConnectionConfiguration connection, IMiddlewareContext context)
		{
			if (connection == null || connection.DataProvider == Guid.Empty)
				return null;

			var dataProvider = context.Tenant.GetService<IDataProviderService>().Select(connection.DataProvider);

			if (dataProvider == null)
				return null;

			var att = dataProvider.GetType().FindAttribute<SchemaBrowserAttribute>();

			if (att == null)
				return null;

			return att.Type == null
				? Reflection.TypeExtensions.GetType(att.TypeName).CreateInstance<ISchemaBrowser>()
				: att.Type.CreateInstance<ISchemaBrowser>();
		}

		public static IConnectionConfiguration DefaultConnection(this IMiddlewareContext context)
		{
			var connections = context.Tenant.GetService<IComponentService>().QueryComponents(context.MicroService.Token, ComponentCategories.Connection);

			if (connections != null && connections.Count == 1)
				return context.Tenant.GetService<IComponentService>().SelectConfiguration(connections[0].Token) as IConnectionConfiguration;

			return null;
		}
	}
}
