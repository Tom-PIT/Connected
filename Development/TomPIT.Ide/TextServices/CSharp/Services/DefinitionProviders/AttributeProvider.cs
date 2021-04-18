using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Ide.Analysis;
using TomPIT.Reflection;

namespace TomPIT.Ide.TextServices.CSharp.Services.DefinitionProviders
{
	internal class AttributeProvider : DefinitionProvider
	{
		protected override Languages.ILocation OnProvideDefinition(DefinitionProviderArgs e)
		{
			var span = e.Editor.Document.GetSpan(e.Position);
			var node = e.Model.SyntaxTree.GetRoot().FindNode(span);

			if (CSharpQuery.IsAssignmentExpression(node))
				return ProvideDefinitionFromAssignment(e, span, node);
			else
				return ProvideDefinitionFromParameter(e, span, node);
		}

		private Languages.ILocation ProvideDefinitionFromAssignment(DefinitionProviderArgs e, TextSpan span, SyntaxNode node)
		{
			var assignment = node.Parent as AssignmentExpressionSyntax;

			if (assignment == null)
				return null;

			var property = assignment.ResolvePropertyInfo(e.Model);

			if (property == null)
				return null;

			var att = property.FindAttribute(typeof(DefinitionProviderAttribute).FullTypeName());

			if (att == null)
				return null;

			var provider = CodeAnalysisExtensions.ResolveDefinitionProvider(att);

			if (provider == null)
				return null;

			return provider.ProvideDefinition(e);
		}

		private Languages.ILocation ProvideDefinitionFromParameter(DefinitionProviderArgs e, TextSpan span, SyntaxNode node)
		{
			if (node is AttributeArgumentListSyntax || node is AttributeArgumentSyntax)
				return ProvideDefinitionFromConstructor(e, span, node);
			else
				return ProvideDefinitionFromMethod(e, span, node);
		}

		private Languages.ILocation ProvideDefinitionFromConstructor(DefinitionProviderArgs e, TextSpan span, SyntaxNode node)
		{
			var att = ResolveAttribute(e, node);

			if (att == null)
				return null;

			var provider = att.Type == null
				? Type.GetType(att.TypeName).CreateInstance<IDefinitionProvider>()
				: att.Type.CreateInstance<IDefinitionProvider>();

			if (provider == null)
				return null;

			return provider.ProvideDefinition(e);
		}

		private DefinitionProviderAttribute ResolveAttribute(DefinitionProviderArgs e, SyntaxNode node)
		{
			var index = -1;
			ParameterInfo[] parameters = null;

			if (node is AttributeArgumentSyntax att)
			{
				var args = CSharpQuery.GetArgumentList(att);
				var ctorInfo = CSharpQuery.GetConstructorInfo(e.Model, CSharpQuery.GetMethodSymbol(e.Model, args));

				if (ctorInfo == null)
					return default;

				index = args.Arguments.IndexOf(att);
				parameters = ctorInfo.GetParameters();
			}
			else if (node is ArgumentSyntax arg)
			{
				var args = CSharpQuery.GetArgumentList(arg);
				var ctorInfo = CSharpQuery.GetConstructorInfo(e.Model, CSharpQuery.GetMethodSymbol(e.Model, args));

				if (ctorInfo == null)
					return default;

				index = args.Arguments.IndexOf(arg);
				parameters = ctorInfo.GetParameters();
			}

			if (index == -1 || parameters == null)
				return default;

			if (index >= parameters.Length)
				return default;

			return parameters[index].GetCustomAttribute<DefinitionProviderAttribute>();
		}
		private Languages.ILocation ProvideDefinitionFromMethod(DefinitionProviderArgs e, TextSpan span, SyntaxNode node)
		{
			if (!(node is ArgumentSyntax args))
				return ProvideDefinitionFromComplexInitializer(e, span, node);

			var invocationSymbol = CSharpQuery.GetInvocationSymbolInfo(e.Model, CSharpQuery.GetArgumentList(args));
			var methodInfo = CSharpQuery.GetMethodInfo(e.Model, CSharpQuery.GetArgumentList(args));

			if (methodInfo == null)
				return ProvideDefinitionFromConstructor(e, span, node);

			var index = CSharpQuery.GetArgumentList(args).Arguments.IndexOf(args);
			var pars = methodInfo.GetParameters();

			if (pars.Length > 0 && methodInfo.GetCustomAttribute<ExtensionAttribute>() != null)
				pars = pars.Skip(1).ToArray();

			if (index >= pars.Length)
				return null;

			var att = pars[index].GetCustomAttribute<DefinitionProviderAttribute>();

			if (att == null)
				return null;

			var provider = att.Type == null
				? Type.GetType(att.TypeName).CreateInstance<IDefinitionProvider>()
				: att.Type.CreateInstance<IDefinitionProvider>();

			if (provider == null)
				return null;

			return provider.ProvideDefinition(e);
		}

		private Languages.ILocation ProvideDefinitionFromComplexInitializer(DefinitionProviderArgs e, TextSpan span, SyntaxNode node)
		{
			if (!(node is LiteralExpressionSyntax))
				return null;

			if (!(node.Parent is InitializerExpressionSyntax init))
				return null;

			if (!(init.Parent is InitializerExpressionSyntax collection))
				return null;

			if (!(collection.Parent is ObjectCreationExpressionSyntax create))
				return null;

			var type = e.Model.GetTypeInfo(create);

			if (type.Type == null || type.Type.ContainingAssembly == null)
				return null;

			var t = Type.GetType(string.Format("{0}.{1}, {2}", type.Type.ContainingNamespace, type.Type.Name, type.Type.ContainingAssembly.Name));

			if (!(t == typeof(JObject)))
				return null;
			//TODO: resolve extenders and similar artifacts
			return null;
		}
	}
}
