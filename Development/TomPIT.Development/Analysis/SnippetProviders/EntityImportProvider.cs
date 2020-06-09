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
using TomPIT.Ide.Analysis.Suggestions;
using TomPIT.Reflection;

namespace TomPIT.Development.Analysis.SnippetProviders
{
	internal class EntityImportProvider : ISnippetProvider
	{
		private const string ValidPropertyCharacters = "abcdefghijklmnoprqstuvzyxw0123456789";
		private const string ValidPropertyCapCharacters = "abcdefghijklmnoprqstuvzyxw";

		private const string ImportStart = "public class ImportWrapper{ public ImportWrapper(){";
		private const string ImportEnd = "}private void import(string connection, string group, string groupObject){}}";
		public List<ISuggestion> ProvideSnippets(SnippetArgs e)
		{
			var root = e.Model.SyntaxTree.GetRoot();
			var node = root.FindNode(e.Span);
			var classScope = node.DeclaredClass();

			if (classScope == null || classScope.LookupBaseType(e.Model, typeof(DataEntity).FullTypeName()) == null)
				return null;

			var token = root.FindToken(e.Position);
			var comment = token.SingleLineComment(e.Span, out int spanStart);

			if (comment == null)
				return null;

			var snippet = CSharpSyntaxTree.ParseText($"{ImportStart}{comment}{ImportEnd}");

			if (string.IsNullOrWhiteSpace(comment))
			{
				return new List<ISuggestion>
				{
					new Suggestion
					{
						Label = "ImportEntity",
						InsertText = "import()",
						SortText = "ImportEntity",
						FilterText = "ImportEntity",
						Kind = Suggestion.Snippet
					}
				};
			}

			return ResolveStatement(e, snippet, e.Position - spanStart + ImportStart.Length);
		}

		private List<ISuggestion> ResolveStatement(SnippetArgs e, SyntaxTree scope, int position)
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
				return ResolveArguments(e, scope, position, list);
			else if (snippetToken.Kind() == SyntaxKind.CloseBraceToken)
			{
				if (snippetToken.GetPreviousToken().Kind() == SyntaxKind.CloseParenToken && snippetToken.GetPreviousToken().Parent is ArgumentListSyntax al)
					return ExecuteSnippet(e, al);
			}

			return null;
		}

		private List<ISuggestion> ExecuteSnippet(SnippetArgs e, ArgumentListSyntax list)
		{
			var browser = ResolveSchemaBrowser(e, ConnectionName(list));

			if (browser.Item1 == null)
				return null;

			var items = browser.Item1.QuerySchema(browser.Item2, SchemaGroupName(list), SchemaGroupObjectName(list));

			if (items == null || items.Count == 0)
			{
				return new List<ISuggestion>
				{
					new Suggestion
					{
						Kind=Suggestion.Text,
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

			return new List<ISuggestion>
			{
				new Suggestion
				{
					FilterText = "Execute",
					Kind = Suggestion.Snippet,
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

		private List<ISuggestion> ResolveArguments(SnippetArgs e, SyntaxTree scope, int position, ArgumentListSyntax list)
		{
			if (list.Arguments.Count == 0)
				return ConnectionsSuggestion(e, list);
			else
			{
				for (var i = 0; i < list.Arguments.Count; i++)
				{
					var parameter = list.Arguments[i];

					if (parameter.Span.IntersectsWith(position))
						return CreateArgumentsSuggestion(e, list, i);
				}
			}

			return null;
		}

		private List<ISuggestion> CreateArgumentsSuggestion(SnippetArgs e, ArgumentListSyntax list, int index)
		{
			if (index == 0)
				return ConnectionsSuggestion(e, list);
			else if (index == 1)
				return SchemaGroupsSuggestion(e, list);
			else if (index == 2)
				return SchemaObjectsSuggestion(e, list);
			else
				return null;
		}

		private List<ISuggestion> SchemaObjectsSuggestion(SnippetArgs e, ArgumentListSyntax list)
		{
			var group = SchemaGroupName(list);

			if (string.IsNullOrWhiteSpace(group))
				return null;

			var browser = ResolveSchemaBrowser(e, ConnectionName(list));

			if (browser.Item1 == null)
				return null;

			var r = new List<ISuggestion>();

			foreach (var groupObject in browser.Item1.QueryGroupObjects(browser.Item2, group))
			{
				r.Add(new Suggestion
				{
					FilterText = groupObject.Text,
					InsertText = $"\"{groupObject.Value}\"",
					Label = groupObject.Text,
					SortText = groupObject.Text,
					Kind = Suggestion.Text
				});
			}

			return r;
		}

		private List<ISuggestion> SchemaGroupsSuggestion(SnippetArgs e, ArgumentListSyntax list)
		{
			var browser = ResolveSchemaBrowser(e, ConnectionName(list));

			if (browser.Item1 == null)
				return null;

			var r = new List<ISuggestion>();

			foreach (var group in browser.Item1.QuerySchemaGroups(browser.Item2))
			{
				r.Add(new Suggestion
				{
					FilterText = group,
					InsertText = $"\"{group}\"",
					Label = group,
					SortText = group,
					Kind = Suggestion.Text
				});
			}

			return r;
		}

		private List<ISuggestion> ConnectionsSuggestion(SnippetArgs e, ArgumentListSyntax list)
		{
			return default;
			//var connections = e.Context.Tenant.GetService<IComponentService>().QueryComponents(e.Context.MicroService.Token, ComponentCategories.Connection);
			//var r = new List<ISuggestion>();

			//foreach (var connection in connections)
			//{
			//	r.Add(new Suggestion
			//	{
			//		FilterText = connection.Name,
			//		Kind = Suggestion.Text,
			//		InsertText = $"\"{e.Context.MicroService.Name}/{connection.Name}\"",
			//		Label = connection.Name,
			//		SortText = connection.Name
			//	});
			//}

			//return r;
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

		private (ISchemaBrowser, IConnectionConfiguration) ResolveSchemaBrowser(SnippetArgs e, string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return (null, null);

			var tokens = name.Split("/");
			var ms = e.Context.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				return (null, null);

			var connection = e.Context.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, ComponentCategories.Connection, tokens[1]) as IConnectionConfiguration;

			if (connection == null)
				return (null, null);

			var cs = connection.ResolveConnectionString(e.Context);
			var provider = e.Context.Tenant.GetService<IDataProviderService>().Select(cs.DataProvider);

			if (provider == null)
				return (null, null);

			var att = provider.GetType().FindAttribute<SchemaBrowserAttribute>();

			if (att == null)
				return (null, null);

			var result = (att.Type == null
				? TypeExtensions.GetType(att.TypeName).CreateInstance<ISchemaBrowser>()
				: att.Type.CreateInstance<ISchemaBrowser>(), connection);

			if (result.Item1 != null)
				ReflectionExtensions.SetPropertyValue(result.Item1, nameof(result.Item1.Context), e.Context);

			return result;
		}
	}
}
