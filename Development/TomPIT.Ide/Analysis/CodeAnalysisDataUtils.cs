using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data;
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
			return ResolveComponent<IConnectionConfiguration>(syntax, context, ComponentCategories.Connection);
		}

		public static IModelConfiguration ResolveModel(this VariableDeclaratorSyntax syntax, IMiddlewareContext context)
		{
			return ResolveComponent<IModelConfiguration>(syntax, context, ComponentCategories.Model);
		}

		private static T ResolveComponent<T>(this VariableDeclaratorSyntax syntax, IMiddlewareContext context, string componentCategory)
		{
			var invocations = syntax.DescendantNodes().OfType<InvocationExpressionSyntax>();

			if (invocations.Count() == 0)
				return default;

			var invocation = invocations.First();

			if (invocation.ArgumentList.Arguments.Count == 0)
				return default;

			if (invocation.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax le)
				return le.ResolveComponent<T>(context, componentCategory);
			else if (invocation.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax identifier)
				return identifier.ResolveComponent<T>(context, componentCategory);
			else
				return default;
		}

		public static IConnectionConfiguration ResolveConnection(this IdentifierNameSyntax ins, IMiddlewareContext context)
		{
			return ResolveComponent<IConnectionConfiguration>(ins, context, ComponentCategories.Connection);
		}
		public static IModelConfiguration ResolveModel(this IdentifierNameSyntax ins, IMiddlewareContext context)
		{
			return ResolveComponent<IModelConfiguration>(ins, context, ComponentCategories.Model);
		}

		private static T ResolveComponent<T>(this IdentifierNameSyntax ins, IMiddlewareContext context, string componentCategory)
		{
			var identifierName = ins.Identifier.Text;

			if (string.IsNullOrWhiteSpace(identifierName))
				return default;

			var declaration = ins.DeclarationScope();

			if (declaration == null)
				return default;

			var variable = declaration.VariableDeclaration(identifierName);

			if (variable == null)
				return default;

			return variable.ResolveComponent<T>(context, componentCategory);
		}

		public static IConnectionConfiguration ResolveConnection(this LiteralExpressionSyntax le, IMiddlewareContext context)
		{
			return ResolveComponent<IConnectionConfiguration>(le, context, ComponentCategories.Connection);
		}

		public static IModelConfiguration ResolveModel(this LiteralExpressionSyntax le, IMiddlewareContext context)
		{
			return ResolveComponent<IModelConfiguration>(le, context, ComponentCategories.Model);
		}

		public static T ResolveComponent<T>(this LiteralExpressionSyntax le, IMiddlewareContext context, string componentCategory)
		{
			var text = le.Token.ValueText;

			if (string.IsNullOrWhiteSpace(text))
				return default;

			var tokens = text.Split('/');

			if (tokens.Length != 2)
				return default;

			var ms = context.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				return default;

			return (T)context.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, componentCategory, tokens[1]);
		}

		public static ISchemaBrowser ResolveSchemaBrowser(this IConnectionConfiguration connection, IMiddlewareContext context)
		{
			if (connection == null)
				return null;

			var cs = connection.ResolveConnectionString(context, ConnectionStringContext.Elevated);
			var dataProvider = context.Tenant.GetService<IDataProviderService>().Select(cs.DataProvider);

			if (dataProvider == null)
				return null;

			var att = dataProvider.GetType().FindAttribute<SchemaBrowserAttribute>();

			if (att == null)
				return null;

			var result = att.Type == null
				? Reflection.TypeExtensions.GetType(att.TypeName).CreateInstance<ISchemaBrowser>()
				: att.Type.CreateInstance<ISchemaBrowser>();

			if (result != null)
				ReflectionExtensions.SetPropertyValue(result, nameof(result.Context), context);

			return result;
		}

		public static IConnectionConfiguration DefaultConnection(this IMicroServiceContext context)
		{
			var connections = context.Tenant.GetService<IComponentService>().QueryComponents(context.MicroService.Token, ComponentCategories.Connection);

			if (connections != null && connections.Count == 1)
				return context.Tenant.GetService<IComponentService>().SelectConfiguration(connections[0].Token) as IConnectionConfiguration;

			return null;
		}
	}
}
