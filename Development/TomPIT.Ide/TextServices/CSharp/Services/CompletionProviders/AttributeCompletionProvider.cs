using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Reflection;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders
{
	internal class AttributeCompletionProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var result = new List<ICompletionItem>();

			var span = Editor.Document.GetSpan(Arguments.Position);

			ProvideItems(result, span);

			return result;
		}

		private void ProvideItems(List<ICompletionItem> items, TextSpan span)
		{
			var node = Arguments.Model.SyntaxTree.GetRoot().FindNode(span);

			if (CSharpQuery.IsAssignmentExpression(node))
				ProvideItemsFromAssignment(items, span, node);
			else
				ProvideItemsFromParameter(items, span, node);
		}

		private void ProvideItemsFromAssignment(List<ICompletionItem> items, TextSpan span, SyntaxNode node)
		{
			var assignment = node.Parent as AssignmentExpressionSyntax;

			if (assignment == null)
				return;

			var property = assignment.ResolvePropertyInfo(Arguments.Model);

			if (property == null)
				return;

			var att = property.FindAttribute(typeof(CompletionItemProviderAttribute).FullTypeName());

			if (att == null)
				return;

			var provider = CodeAnalysisExtensions.ResolveCompletionProvider(att);

			if (provider == null)
				return;

			var result = provider.ProvideItems(Arguments);

			if (result != null && result.Count > 0)
				items.AddRange(result);
		}

		private void ProvideItemsFromParameter(List<ICompletionItem> items, TextSpan span, SyntaxNode node)
		{
			if (node is AttributeArgumentListSyntax || node is AttributeArgumentSyntax)
				ProvideItemsFromConstructor(items, span, node);
			else
				ProvideItemsFromMethod(items, span, node);
		}

		private void ProvideItemsFromConstructor(List<ICompletionItem> items, TextSpan span, SyntaxNode node)
		{
			var att = ResolveAttribute(node);

			if (att == null)
				return;

			var provider = att.Type == null
				? Type.GetType(att.TypeName).CreateInstance<ICompletionProvider>()
				: att.Type.CreateInstance<ICompletionProvider>();

			if (provider == null)
				return;

			var result = provider.ProvideItems(Arguments);

			if (result != null && result.Count > 0)
				items.AddRange(result);
		}

		private CompletionItemProviderAttribute ResolveAttribute(SyntaxNode node)
		{
			var index = -1;
			ParameterInfo[] parameters = null;

			if (node is AttributeArgumentSyntax att)
			{
				var args = CSharpQuery.GetArgumentList(att);
				var ctorInfo = CSharpQuery.GetConstructorInfo(Arguments.Model, CSharpQuery.GetMethodSymbol(Arguments.Model, args));

				if (ctorInfo == null)
					return default;

				index = args.Arguments.IndexOf(att);
				parameters = ctorInfo.GetParameters();
			}
			else if (node is ArgumentSyntax arg)
			{
				var args = CSharpQuery.GetArgumentList(arg);
				var ctorInfo = CSharpQuery.GetConstructorInfo(Arguments.Model, CSharpQuery.GetMethodSymbol(Arguments.Model, args));

				if (ctorInfo == null)
					return default;

				index = args.Arguments.IndexOf(arg);
				parameters = ctorInfo.GetParameters();
			}

			if (index == -1 || parameters == null)
				return default;

			if (index >= parameters.Length)
				return default;

			return parameters[index].GetCustomAttribute<CompletionItemProviderAttribute>();
		}
		private void ProvideItemsFromMethod(List<ICompletionItem> items, TextSpan span, SyntaxNode node)
		{
			if (!(node is ArgumentSyntax args))
			{
				ProvideItemsFromComplexInitializer(items, span, node);
				return;
			}

			var invocationSymbol = CSharpQuery.GetInvocationSymbolInfo(Arguments.Model, CSharpQuery.GetArgumentList(args));
			var methodInfo = CSharpQuery.GetMethodInfo(Arguments.Model, CSharpQuery.GetArgumentList(args));

			if (methodInfo == null)
			{
				ProvideItemsFromConstructor(items, span, node);
				return;
			}

			var index = CSharpQuery.GetArgumentList(args).Arguments.IndexOf(args);
			var pars = methodInfo.GetParameters();

			if (pars.Length > 0 && methodInfo.GetCustomAttribute<ExtensionAttribute>() != null)
				pars = pars.Skip(1).ToArray();

			if (index >= pars.Length)
				return;

			var att = pars[index].GetCustomAttribute<CompletionItemProviderAttribute>();

			if (att == null)
				return;

			var provider = att.Type == null
				? Type.GetType(att.TypeName).CreateInstance<ICompletionProvider>()
				: att.Type.CreateInstance<ICompletionProvider>();

			if (provider == null)
				return;

			var result = provider.ProvideItems(Arguments);

			if (result != null && result.Count > 0)
				items.AddRange(result);
		}

		private void ProvideItemsFromComplexInitializer(List<ICompletionItem> items, TextSpan span, SyntaxNode node)
		{
			if (!(node is LiteralExpressionSyntax))
				return;

			if (!(node.Parent is InitializerExpressionSyntax init))
				return;

			if (!(init.Parent is InitializerExpressionSyntax collection))
				return;

			if (!(collection.Parent is ObjectCreationExpressionSyntax create))
				return;

			var type = Arguments.Model.GetTypeInfo(create);

			if (type.Type == null || type.Type.ContainingAssembly == null)
				return;

			var t = Type.GetType(string.Format("{0}.{1}, {2}", type.Type.ContainingNamespace, type.Type.Name, type.Type.ContainingAssembly.Name));

			if (!(t == typeof(JObject)))
				return;

			ProvideItems(items, create.Parent.Span);
		}
	}
}
