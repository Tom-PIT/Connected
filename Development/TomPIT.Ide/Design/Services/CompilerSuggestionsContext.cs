using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Design.Services
{
	internal class CompilerSuggestionsContext : Analyzer<CodeStateArgs>
	{
		private List<ISuggestion> _suggestions = null;
		private SourceText _sourceText = null;

		public CompilerSuggestionsContext(IExecutionContext context, CodeStateArgs e) : base(context, e)
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
				{
					if (Args.Configuration is IPartialSourceCode ps)
					{
						var container = ps.Closest<ISourceCodeContainer>();

						if (container != null)
						{
							var refs = container.References(ps);

							if (refs.Count > 0)
							{
								var sb = new StringBuilder();

								foreach (var i in refs)
								{
									var r1 = container.GetReference(i);
									var txt = Context.Connection().GetService<IComponentService>().SelectText(MicroService, r1);

									if (string.IsNullOrWhiteSpace(txt))
										continue;

									sb.AppendLine();
									sb.Append(txt);
								}

								if (sb.Length > 0)
									_sourceText = SourceText.From(string.Format("{0}{1}", Args.Text, sb.ToString()));
							}
						}
					}

					if (_sourceText == null)
						return SourceText.From(Args.Text);
				}

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
				return WithSnippets(SuggestContent(Document, sm, span), sm, span);

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

			return WithSnippets(r, sm, span);
		}

		private List<ISuggestion> WithSnippets(List<ISuggestion> items, SemanticModel model, TextSpan span)
		{
			var node = model.SyntaxTree.GetRoot().FindNode(span);

			var list = node as ArgumentListSyntax;

			if (list == null)
			{
				if (node is ArgumentSyntax arg)
					list = GetArgumentList(arg);
			}

			if (list == null)
				return items;

			var si = GetInvocationSymbolInfo(model, list);
			var mi = GetMethodInfo(model, list);

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
			var snippets = provider.ProvideSnippets(Context, new CodeAnalysisArgs(Args.Component, model, node, si, text));

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

			if (!(node is ArgumentSyntax args))
				return SuggestComplexInitializer(doc, model, span, node);

			var si = GetInvocationSymbolInfo(model, GetArgumentList(args));
			var methodInfo = GetMethodInfo(model, GetArgumentList(args));

			if (methodInfo == null)
				return null;

			int index = GetArgumentList(args).Arguments.IndexOf(args);

			var pars = methodInfo.GetParameters();

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

			var text = ParseExpressionText(args.Expression);
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
					Kind = Suggestion.Text,
					Label = i.Text
				});
			}


			r.Add(new Suggestion
			{

			});

			return r;
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
	}
}
