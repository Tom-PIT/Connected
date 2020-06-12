using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.DataProviders.Design;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Reflection;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders.Snippets
{
	internal class EntityImportSnippetProvider : SnippetProvider
	{
		private const string ValidPropertyCharacters = "abcdefghijklmnoprqstuvzyxw0123456789";
		private const string ValidPropertyCapCharacters = "abcdefghijklmnoprqstuvzyxw";

		private const string ImportStart = "public class ImportWrapper{ public ImportWrapper(){";
		private const string ImportEnd = "}private void import(string connection, string group, string groupObject){}}";

		protected override List<ICompletionItem> OnProvideItems()
		{
			var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(Editor.Document);
			var position = Editor.Document.GetCaret(Arguments.Position);
			var span = service.GetDefaultCompletionListSpan(Editor.SourceText, position);
			var root = Arguments.Model.SyntaxTree.GetRoot();
			var node = root.FindNode(span);
			var classScope = node.DeclaredClass();

			if (classScope == null || classScope.LookupBaseType(Arguments.Model, typeof(DataEntity).FullTypeName()) == null)
				return null;

			var token = root.FindToken(position);
			var comment = token.SingleLineComment(span, out int spanStart);

			if (comment == null)
				return null;

			var snippet = CSharpSyntaxTree.ParseText($"{ImportStart}{comment}{ImportEnd}");

			if (string.IsNullOrWhiteSpace(comment))
			{
				return new List<ICompletionItem>
				{
					new CompletionItem
					{
						Label = "ImportEntity",
						InsertText = "import()",
						SortText = "ImportEntity",
						FilterText = "ImportEntity",
						Kind = CompletionItemKind.Snippet
					}
				};
			}

			return ResolveStatement(snippet, position - spanStart + ImportStart.Length);
		}

		private List<ICompletionItem> ResolveStatement(SyntaxTree scope, int position)
		{
			SyntaxToken snippetToken;

			try
			{
				snippetToken = scope.GetRoot().FindToken(position);
			}
			catch
			{
				return null;
			}

			if ((snippetToken.Parent is ArgumentListSyntax list))
				return ResolveArguments(scope, position, list);
			else if (snippetToken.Kind() == SyntaxKind.CloseBraceToken)
			{
				if (snippetToken.GetPreviousToken().Kind() == SyntaxKind.CloseParenToken && snippetToken.GetPreviousToken().Parent is ArgumentListSyntax al)
					return ExecuteSnippet(al);
			}

			return null;
		}

		private List<ICompletionItem> ExecuteSnippet(ArgumentListSyntax list)
		{
			var browser = ResolveSchemaBrowser(ConnectionName(list));

			if (browser.Item1 == null)
				return null;

			var items = browser.Item1.QuerySchema(browser.Item2, SchemaGroupName(list), SchemaGroupObjectName(list));

			if (items == null || items.Count == 0)
			{
				return new List<ICompletionItem>
				{
					new CompletionItem
					{
						Kind= CompletionItemKind.Text,
						Label="No schema available"
					}
				};
			}

			var sb = new StringBuilder();

			sb.AppendLine();

			foreach (var item in items.OrderBy(f => f.Name))
			{
				if (string.IsNullOrWhiteSpace(item.Name))
					continue;

				var identifier = ValidateIdentifier(item.Name);

				if (identifier.Item2.Length == 0)
					continue;

				var cap = identifier.Item2[0].ToString().ToUpper();
				var text = identifier.Item2.Length < 2 ? string.Empty : identifier.Item2.Substring(1);

				var name = $"{cap}{text}";

				if (!identifier.Item1)
					sb.AppendLine($"[Mapping(\"{item.Name}\")]");

				sb.AppendLine($"public {Types.ToType(item.DataType).ToFriendlyName()} {name} {{get;set;}}");
			}

			return new List<ICompletionItem>
			{
				new CompletionItem
				{
					FilterText = "Execute",
					Kind = CompletionItemKind.Snippet,
					InsertText = sb.ToString(),
					Label = "Execute",
					SortText = "Execute"
				}
			};
		}

		private (bool, string) ValidateIdentifier(string value)
		{
			var r = new StringBuilder();
			var pass = true;

			for (var i = 0; i < value.Length; i++)
			{
				if (i == 0)
				{
					if (!ValidPropertyCapCharacters.Contains(value[i].ToString().ToLowerInvariant()))
						pass = false;
					else
						r.Append(value[i]);
				}
				else
				{
					if (!ValidPropertyCapCharacters.Contains(value[i].ToString().ToLowerInvariant()))
						pass = false;
					else
						r.Append(value[i]);
				}
			}

			return (pass, r.ToString());
		}

		private List<ICompletionItem> ResolveArguments(SyntaxTree scope, int position, ArgumentListSyntax list)
		{
			if (list.Arguments.Count == 0)
				return ConnectionsSuggestion(list);
			else
			{
				for (var i = 0; i < list.Arguments.Count; i++)
				{
					var parameter = list.Arguments[i];

					if (parameter.Span.IntersectsWith(position))
						return CreateArgumentsSuggestion(list, i);
				}
			}

			return null;
		}

		private List<ICompletionItem> CreateArgumentsSuggestion(ArgumentListSyntax list, int index)
		{
			if (index == 0)
				return ConnectionsSuggestion(list);
			else if (index == 1)
				return SchemaGroupsSuggestion(list);
			else if (index == 2)
				return SchemaObjectsSuggestion(list);
			else
				return null;
		}

		private List<ICompletionItem> SchemaObjectsSuggestion(ArgumentListSyntax list)
		{
			var group = SchemaGroupName(list);

			if (string.IsNullOrWhiteSpace(group))
				return null;

			var browser = ResolveSchemaBrowser(ConnectionName(list));

			if (browser.Item1 == null)
				return null;

			var r = new List<ICompletionItem>();

			foreach (var groupObject in browser.Item1.QueryGroupObjects(browser.Item2, group))
			{
				r.Add(new CompletionItem
				{
					FilterText = groupObject.Text,
					InsertText = $"\"{groupObject.Value}\"",
					Label = groupObject.Text,
					SortText = groupObject.Text,
					Kind = CompletionItemKind.Text
				});
			}

			return r;
		}

		private List<ICompletionItem> SchemaGroupsSuggestion(ArgumentListSyntax list)
		{
			var browser = ResolveSchemaBrowser(ConnectionName(list));

			if (browser.Item1 == null)
				return null;

			var r = new List<ICompletionItem>();

			foreach (var group in browser.Item1.QuerySchemaGroups(browser.Item2))
			{
				r.Add(new CompletionItem
				{
					FilterText = group,
					InsertText = $"\"{group}\"",
					Label = group,
					SortText = group,
					Kind = CompletionItemKind.Text
				});
			}

			return r;
		}

		private List<ICompletionItem> ConnectionsSuggestion(ArgumentListSyntax list)
		{
			var connections = Editor.Context.Tenant.GetService<IComponentService>().QueryComponents(Editor.Context.MicroService.Token, ComponentCategories.Connection);
			var r = new List<ICompletionItem>();

			foreach (var connection in connections)
			{
				r.Add(new CompletionItem
				{
					FilterText = connection.Name,
					Kind = CompletionItemKind.Text,
					InsertText = $"\"{Editor.Context.MicroService.Name}/{connection.Name}\"",
					Label = connection.Name,
					SortText = connection.Name
				});
			}

			return r;
		}

		private string ConnectionName(ArgumentListSyntax list)
		{
			if (list.Arguments.Count == 0)
				return null;

			var le = list.Arguments[0].Expression as LiteralExpressionSyntax;

			return le.Token.ValueText;
		}

		private string SchemaGroupName(ArgumentListSyntax list)
		{
			if (list.Arguments.Count < 2)
				return null;

			var le = list.Arguments[1].Expression as LiteralExpressionSyntax;

			return le.Token.ValueText;
		}

		private string SchemaGroupObjectName(ArgumentListSyntax list)
		{
			if (list.Arguments.Count < 3)
				return null;

			var le = list.Arguments[2].Expression as LiteralExpressionSyntax;

			return le.Token.ValueText;
		}

		private (ISchemaBrowser, IConnectionConfiguration) ResolveSchemaBrowser(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return (null, null);

			var tokens = name.Split("/");
			var ms = Editor.Context.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				return (null, null);

			var connection = Editor.Context.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, ComponentCategories.Connection, tokens[1]) as IConnectionConfiguration;

			if (connection == null)
				return (null, null);

			var cs = connection.ResolveConnectionString(Editor.Context, ConnectionStringContext.Elevated);
			var provider = Editor.Context.Tenant.GetService<IDataProviderService>().Select(cs.DataProvider);

			if (provider == null)
				return (null, null);

			var att = provider.GetType().FindAttribute<SchemaBrowserAttribute>();

			if (att == null)
				return (null, null);

			var result = (att.Type == null
				? TypeExtensions.GetType(att.TypeName).CreateInstance<ISchemaBrowser>()
				: att.Type.CreateInstance<ISchemaBrowser>(), connection);

			if (result.Item1 != null)
				ReflectionExtensions.SetPropertyValue(result.Item1, nameof(result.Item1.Context), Arguments.Editor.Context);

			return result;
		}
	}
}