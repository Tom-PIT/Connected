﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Ide.Analysis.Analyzers;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Ide.Analysis.Suggestions
{
	internal class CSharpSuggestionsContext : CSharpCodeAnalyzer<CodeStateArgs>
	{
		private List<ISuggestion> _suggestions = null;
		private SourceText _sourceText = null;

		public CSharpSuggestionsContext(IMiddlewareContext context, CodeStateArgs e) : base(context, e)
		{

		}


		public List<ISuggestion> Suggestions
		{
			get
			{
				if (_suggestions == null)
				{
					_suggestions = CreateSuggestions();

					if (_suggestions == null || _suggestions.Count == 0)
					{
						_suggestions = new List<ISuggestion>
						{
							new Suggestion
							{
								InsertText=string.Empty,
								FilterText=string.Empty,
								Label="no suggestions",
								Kind=Suggestion.Text
							}
						};
					}
				}

				return _suggestions;
			}
		}

		public override SourceText SourceCode
		{
			get
			{
				if (_sourceText == null)
					_sourceText = SourceText.From(Args.Text);

				return _sourceText;
			}
		}

		private List<ISuggestion> CreateSuggestions()
		{
			var results = Task.Run(async () =>
			{
				return await Completion.GetCompletionsAsync(Document, Args.Position);
			}).Result;

			var sm = Task.Run(async () => { return await Document.GetSemanticModelAsync(); }).Result;
			var span = Completion.GetDefaultCompletionListSpan(SourceCode, Args.Position);

			if (results == null)
			{
				var scripts = SuggestScripts(Document, sm, span);

				if (scripts != null)
					return scripts;
			}

			if (results == null)
				return WithSnippets(SuggestContent(Document, sm, span), new SnippetArgs(Context, sm, span, Args.Position));

			var ti = sm.GetTypeInfo(sm.SyntaxTree.GetRoot().FindNode(results.Span));

			var r = new List<ISuggestion>();
			var fd = sm.SyntaxTree.GetRoot().FindNode(results.Span);

			foreach (var i in results.Items)
			{
				CompletionDescription description = null;
				var kind = ResolveKind(i);

				if (results.Items.Length < 50 || Suggestion.SupportsDescription(kind))
					description = Task.Run(async () => { return await Completion.GetDescriptionAsync(Document, i); }).Result;

				var s = new Suggestion
				{
					Description = description == null ? string.Empty : description.Text,
					InsertText = i.DisplayText,
					Kind = ResolveKind(i),
					FilterText = i.FilterText,
					SortText = i.SortText,
					Label = i.DisplayText
				};

				foreach (var j in i.Rules.CommitCharacterRules)
				{
					foreach (var k in j.Characters)
						s.CommitCharacters.Add(k.ToString());
				}

				r.Add(s);
			}

			return WithSnippets(r, new SnippetArgs(Context, sm, span, Args.Position));
		}

		private List<ISuggestion> WithSnippets(List<ISuggestion> items, SnippetArgs e)
		{
			if (items == null)
				items = new List<ISuggestion>();

			var systemSnippets = Context.Tenant.GetService<ICodeAnalysisService>().ProvideSnippets(e);

			if (systemSnippets != null && systemSnippets.Count > 0)
				items.AddRange(systemSnippets);

			var node = e.Model.SyntaxTree.GetRoot().FindNode(e.Span);

			var list = node as ArgumentListSyntax;

			if (list == null)
			{
				if (node is ArgumentSyntax arg)
					list = GetArgumentList(arg);
			}

			if (list == null)
				return items;

			var si = GetInvocationSymbolInfo(e.Model, list);
			var mi = GetMethodInfo(e.Model, list);

			if (mi == null)
				return items;

			var activeParameter = 0;

			foreach (var i in list.Arguments)
			{
				if (i.Span.End < Args.Position)
					activeParameter++;
				else
					break;
			}

			var pars = mi.GetParameters();

			if (activeParameter >= pars.Length)
				return items;

			var att = pars[activeParameter].GetCustomAttribute<CodeAnalysisProviderAttribute>();

			if (att == null)
				return items;

			var provider = att.Type == null
				? Type.GetType(att.TypeName).CreateInstance<ICodeAnalysisProvider>(new object[] { Context })
				: att.Type.CreateInstance<ICodeAnalysisProvider>(new object[] { Context });

			if (provider == null)
				return items;

			var text = string.Empty;// ParseExpressionText(list.Expression);
			var snippets = provider.ProvideSnippets(Context, new CodeAnalysisArgs(Args.Component, e.Model, node, si, text));

			if (snippets != null && snippets.Count > 0)
			{
				if (items == null)
					items = new List<ISuggestion>();

				foreach (var i in snippets)
				{
					items.Add(new Suggestion
					{
						Description = i.Description,
						FilterText = i.Text,
						InsertText = i.Value,
						Kind = Suggestion.Snippet,
						Label = i.Text,
						SortText = i.Text
					});
				}
			}

			return items;
		}

		private List<ISuggestion> SuggestContent(Document doc, SemanticModel model, TextSpan span)
		{
			var node = model.SyntaxTree.GetRoot().FindNode(span);

			if (IsAssignmentExpression(node))
				return SuggestPropertyValues(doc, model, span, node);

			MethodInfo methodInfo = null;
			ConstructorInfo ctorInfo = null;
			var index = 0;
			SymbolInfo si = new SymbolInfo();
			var text = string.Empty;

			if (node is AttributeArgumentSyntax aargs)
			{
				si = GetInvocationSymbolInfo(model, GetArgumentList(aargs));
				ctorInfo = GetConstructorInfo(model, GetMethodSymbol(model, GetArgumentList(aargs)));
				index = GetArgumentList(aargs).Arguments.IndexOf(aargs);
				text = ParseExpressionText(aargs.Expression);
			}
			else
			{
				if (!(node is ArgumentSyntax args))
					return SuggestComplexInitializer(doc, model, span, node);

				si = GetInvocationSymbolInfo(model, GetArgumentList(args));
				methodInfo = GetMethodInfo(model, GetArgumentList(args));

				if (methodInfo == null)
					return null;

				index = GetArgumentList(args).Arguments.IndexOf(args);
				text = ParseExpressionText(args.Expression);
			}

			ParameterInfo[] pars = null;

			if (methodInfo == null)
				pars = ctorInfo.GetParameters();
			else
			{
				pars = methodInfo.GetParameters();

				if (pars.Length > 0 && methodInfo.GetCustomAttribute<ExtensionAttribute>() != null)
					pars = pars.Skip(1).ToArray();
			}

			if (index >= pars.Length)
				return null;

			var att = pars[index].GetCustomAttribute<CodeAnalysisProviderAttribute>();

			if (att == null)
				return null;

			var provider = att.Type == null
				? Type.GetType(att.TypeName).CreateInstance<ICodeAnalysisProvider>(new object[] { Context })
				: att.Type.CreateInstance<ICodeAnalysisProvider>(new object[] { Context });

			if (provider == null)
				return null;

			var items = provider.ProvideLiterals(Context, new CodeAnalysisArgs(Args.Component, model, node, si, text));

			if (items == null)
				return new List<ISuggestion>();

			var r = new List<ISuggestion>();

			foreach (var i in items)
			{
				r.Add(new Suggestion
				{
					Description = i.Description,
					InsertText = i.Value,
					FilterText = i.Text,
					SortText = i.Text,
					Kind = Suggestion.Text,
					Label = i.Text
				});
			}

			return r;
		}

		private List<ISuggestion> SuggestPropertyValues(Document doc, SemanticModel model, TextSpan span, SyntaxNode node)
		{
			var assignment = node.Parent as AssignmentExpressionSyntax;

			if (assignment == null)
				return null;

			var property = assignment.ResolvePropertyInfo(model);

			if (property == null)
				return null;

			var att = property.FindAttribute(typeof(CompletionItemProviderAttribute).FullName);

			if (att == null)
				return null;

			var items = new List<ISuggestion>();

			var provider = CodeAnalysisExtensions.ResolveCompletionProvider(att);

			return null;
			//if (provider == null)
			//	return items;

			//var result = provider.ProvideItems(Arguments);

			//if (result != null && result.Count > 0)
			//	items.AddRange(result);

			//var literals = provider.ProvideLiterals(Context, new CodeAnalysisArgs(Args.Component, model, node, model.GetSymbolInfo(assignment), null));

			//if (literals != null && literals.Count > 0)
			//{
			//	foreach (var i in literals)
			//	{
			//		items.Add(new Suggestion
			//		{
			//			Description = i.Description,
			//			FilterText = i.Text,
			//			InsertText = i.Value,
			//			Kind = Suggestion.Text,
			//			Label = i.Text,
			//			SortText = i.Text
			//		});
			//	}
			//}

			//return items;
		}

		private bool IsAssignmentExpression(SyntaxNode node)
		{
			return node.Parent is AssignmentExpressionSyntax;
		}

		private List<ISuggestion> SuggestComplexInitializer(Document doc, SemanticModel model, TextSpan span, SyntaxNode node)
		{
			if (!(node is LiteralExpressionSyntax))
				return null;

			if (!(node.Parent is InitializerExpressionSyntax init))
				return null;

			if (!(init.Parent is InitializerExpressionSyntax collection))
				return null;

			if (!(collection.Parent is ObjectCreationExpressionSyntax create))
				return null;

			var type = model.GetTypeInfo(create);

			if (type.Type == null || type.Type.ContainingAssembly == null)
				return null;

			var t = Type.GetType(string.Format("{0}.{1}, {2}", type.Type.ContainingNamespace, type.Type.Name, type.Type.ContainingAssembly.Name));

			if (!(t == typeof(JObject)))
				return null;

			return SuggestContent(doc, model, create.Parent.Span);
		}

		private bool IsPublic(CompletionItem item)
		{
			if (item.Tags == null || item.Tags.Length < 2)
				return false;

			var value = item.Tags[1];

			return string.Compare(value, "Public", true) == 0;
		}

		private int ResolveKind(CompletionItem item)
		{
			if (item.Tags == null || item.Tags.Length == 0)
				return -1;

			var value = item.Tags[0];

			if (string.Compare("Variable", value, true) == 0)
				return Suggestion.Variable;
			else if (string.Compare("Value", value, true) == 0)
				return Suggestion.Value;//boolean
			else if (string.Compare("Class", value, true) == 0)
				return Suggestion.Class;
			else if (string.Compare("Constant", value, true) == 0)
				return Suggestion.Constant;
			else if (string.Compare("Constructor", value, true) == 0)
				return Suggestion.Constructor;
			else if (string.Compare("Enum", value, true) == 0)
				return Suggestion.Enum;
			else if (string.Compare("EnumMember", value, true) == 0)
				return Suggestion.EnumMember;
			else if (string.Compare("Event", value, true) == 0)
				return Suggestion.Event;
			else if (string.Compare("Field", value, true) == 0)
				return Suggestion.Field;
			else if (string.Compare("File", value, true) == 0)
				return Suggestion.File;
			else if (string.Compare("Function", value, true) == 0)
				return Suggestion.Function;
			else if (string.Compare("Interface", value, true) == 0)
				return Suggestion.Interface;
			else if (string.Compare("Keyword", value, true) == 0)
				return Suggestion.Keyword;
			else if (string.Compare("Method", value, true) == 0 || string.Compare("ExtensionMethod", value, true) == 0)
				return Suggestion.Method;
			else if (string.Compare("Module", value, true) == 0)
				return Suggestion.Module;
			else if (string.Compare("Namespace", value, true) == 0)
				return Suggestion.Module;//namespace
			else if (string.Compare("Class", value, true) == 0)
				return Suggestion.Class;
			else if (string.Compare("Null", value, true) == 0)
				return Suggestion.Value;//null
			else if (string.Compare("Number", value, true) == 0)
				return Suggestion.Value;//number
			else if (string.Compare("Reference", value, true) == 0)
				return Suggestion.Reference;//object
			else if (string.Compare("Operator", value, true) == 0)
				return Suggestion.Operator;
			else if (string.Compare("Package", value, true) == 0)
				return Suggestion.Module;//package
			else if (string.Compare("Property", value, true) == 0)
				return Suggestion.Property;
			else if (string.Compare("String", value, true) == 0)
				return Suggestion.Value;//string
			else if (string.Compare("Structure", value, true) == 0)
				return Suggestion.Struct;
			else if (string.Compare("Type", value, true) == 0)
				return Suggestion.TypeParameter;
			else if (string.Compare("Variable", value, true) == 0)
				return Suggestion.Variable;

			return -1;
		}

		private List<ISuggestion> SuggestScripts(Document document, SemanticModel model, TextSpan span)
		{
			var r = new List<ISuggestion>();

			var loads = model.SyntaxTree.GetRoot().FindTrivia(span.Start);

			if (loads != null && loads.IsKind(SyntaxKind.LoadDirectiveTrivia))
			{
				var scripts = Context.Tenant.GetService<IComponentService>().QueryComponents(MicroService, "Script");

				foreach (var script in scripts)
					AddReference(r, script.Name);

				var apis = Context.Tenant.GetService<IComponentService>().QueryConfigurations(MicroService, "Api");

				foreach (IApiConfiguration api in apis)
				{
					var apiName = api.ComponentName();

					foreach (var operation in api.Operations)
						AddReference(r, $"{apiName}/{operation.Name}");
				}

				var refs = Context.Tenant.GetService<IDiscoveryService>().References(MicroService);

				foreach (var reference in refs.MicroServices)
				{
					var ms = Context.Tenant.GetService<IMicroServiceService>().Select(reference.MicroService);

					if (ms == null)
						continue;

					scripts = Context.Tenant.GetService<IComponentService>().QueryComponents(ms.Token, "Script");

					foreach (var script in scripts)
						AddReference(r, $"{ms.Name}/{script.Name}");

					apis = Context.Tenant.GetService<IComponentService>().QueryConfigurations(ms.Token, "Api");

					foreach (IApiConfiguration api in apis)
					{
						var apiName = api.ComponentName();

						foreach (var operation in api.Operations)
							AddReference(r, $"{ms.Name}/{apiName}/{operation.Name}");
					}
				}
			}

			if (r.Count == 0)
				return null;

			return r;
		}

		private void AddReference(List<ISuggestion> items, string identifier)
		{
			var s = new Suggestion
			{
				FilterText = identifier,
				InsertText = $"\"{identifier}\"",
				Kind = Suggestion.Reference,
				Label = identifier,
				SortText = identifier
			};

			s.CommitCharacters.AddRange(new List<string> { "\t", "\"", "\r" });

			items.Add(s);
		}
	}
}