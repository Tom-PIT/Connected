﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Exceptions;
using TomPIT.Ide.TextServices.CSharp;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Reflection;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class ApiOperationParameterProvider : CompletionProvider
	{
		public override bool WillProvideItems(CompletionProviderArgs e, IReadOnlyCollection<ICompletionItem> existing)
		{
			return false;
		}
		protected override List<ICompletionItem> OnProvideItems()
		{
			return null;
			//var node = Arguments.Model.SyntaxTree.GetRoot().FindNode(Editor.GetMappedSpan(Arguments.Position));
			//var descriptor = ResolveApi(node);

			//if (descriptor == null)
			//	return null;

			//var op = descriptor.Configuration.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			//if (op == null)
			//	return null;

			//var parameters = QueryParameters(op);
			//var existing = QueryExisting(node);
			//var r = new List<ICompletionItem>();

			//var members = parameters.DeclaredType?.Members;

			//if (members == null || !members.Any())
			//{
			//	r.Add(new CompletionItem
			//	{
			//		Label = "Api operation has no parameters",
			//		Kind = CompletionItemKind.Text
			//	});
			//}
			//else
			//{
			//	foreach (var parameter in members)
			//	{
			//		if (existing != null && existing.FirstOrDefault(f => string.Compare(f, parameter.Name, true) == 0) != null)
			//			continue;

			//		r.Add(new CompletionItem
			//		{
			//			Label = parameter.Name,
			//			InsertText = parameter.Name,
			//			FilterText = parameter.Name,
			//			SortText = parameter.Name,
			//			Kind = CompletionItemKind.Property,
			//			Detail = parameter.Type
			//		});
			//	}

			//	if (r.Count == 0)
			//	{
			//		r.Add(new CompletionItem
			//		{
			//			Label = "All parameters set",
			//			Kind = CompletionItemKind.Text
			//		});
			//	}
			//else
			//{
			//	if (node is ArgumentSyntax arg && arg.Expression is AnonymousObjectCreationExpressionSyntax)
			//		r.Add(AllParametersSnippet(parameters));
			//}
			//}
			//return r;
		}

		private ConfigurationDescriptor<IApiConfiguration> ResolveApi(SyntaxNode node)
		{
			if (node == null)
				return default;

			var arg = node.Closest<ArgumentListSyntax>();

			if (arg == null)
				return null;

			if (!(arg.Parent is InvocationExpressionSyntax invoke))
				return null;

			if (invoke.ArgumentList.Arguments.Count < 1)
				return null;

			var text = invoke.ArgumentList.Arguments[0].GetText().ToString().Trim().Trim('"');

			if (string.IsNullOrWhiteSpace(text))
				return null;

			var descriptor = ComponentDescriptor.Api(Editor.Context, text);

			try
			{
				descriptor.Validate();
			}
			catch (RuntimeException)
			{
				return null;
			}

			return descriptor;
		}
		private List<string> QueryExisting(SyntaxNode node)
		{
			var arg = node.Closest<ArgumentSyntax>();

			if (arg == null)
				return null;

			var create = arg.ChildNodes().FirstOrDefault(f => f is ObjectCreationExpressionSyntax);

			if (create == null)
				return null;

			var initializer = create.ChildNodes().FirstOrDefault(f => f is InitializerExpressionSyntax);

			if (initializer == null)
				return null;

			var r = new List<string>();

			foreach (var i in initializer.ChildNodes())
			{
				if (!(i is InitializerExpressionSyntax expr) || expr.Expressions.Count == 0)
					continue;

				r.Add(expr.Expressions[0].GetText().ToString().Trim('"').ToLowerInvariant());
			}

			if (r.Count == 0)
				return null;

			return r;
		}
		//private ICompletionItem AllParametersSnippet(ApiOperationManifest config)
		//{
		//	var sb = new StringBuilder();
		//	var properties = SyntaxBrowser.QueryProperties()
		//	var members = config.DeclaredType?.Members.OrderBy(f => f.Name);

		//	if (members != null)
		//	{
		//		foreach (var i in members)
		//		{
		//			var t = TypeExtensions.GetType(i.Type);
		//			object value = "null";

		//			if (t != null)
		//				value = t.DefaultValue();

		//			sb.AppendLine($"{i.Name} = {value},");
		//		}
		//	}

		//	return new CompletionItem
		//	{
		//		Label = "AllParameters",
		//		InsertText = $"{RemoveTrailingComma(sb)}",
		//		Detail = "Insert all api parameters",
		//		FilterText = "AllParameters",
		//		Kind = CompletionItemKind.Snippet
		//	};
		//}
		private bool IsArguments(SemanticModel model, InvocationExpressionSyntax node)
		{
			var idn = node.IdentiferName();

			if (idn == null)
				return false;

			if (string.Compare(idn.Identifier.Text, "e", false) != 0)
				return false;

			var ti = model.GetSymbolInfo(idn);

			if (ti.Symbol == null || ti.Symbol.OriginalDefinition == null)
				return false;

			if (string.Compare(ti.Symbol.OriginalDefinition.ToDisplayString(), "TomPIT.Compilation.ScriptGlobals<T>.e", false) != 0)
				return false;

			return true;
		}

		//private ApiOperationManifest QueryParameters(IApiOperation operation)
		//{
		//	var manifest = Editor.Context.Tenant.GetService<IDiscoveryService>().Manifests.Select(operation.Configuration().Component) as ApiManifest;

		//	if (manifest == null)
		//		return null;

		//	return manifest.Operations.FirstOrDefault(f => string.Compare(operation.Name, f.Name, true) == 0);
		//}

		private string RemoveTrailingComma(StringBuilder sb)
		{
			var idx = sb.Length - 3;

			if (idx < 0)
				return sb.ToString();

			if (sb[idx] == ',')
				return sb.Remove(sb.Length - 3, 1).ToString();

			return sb.ToString();
		}
	}
}
