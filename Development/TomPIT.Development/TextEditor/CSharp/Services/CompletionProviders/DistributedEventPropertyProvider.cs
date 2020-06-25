using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Ide.TextServices.CSharp;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class DistributedEventPropertyProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var node = Arguments.Model.SyntaxTree.GetRoot().FindNode(Editor.GetMappedSpan(Arguments.Position));

			if (!(node is ArgumentSyntax arg))
				return null;

			if (!(arg.Expression is AnonymousObjectCreationExpressionSyntax anon))
				return null;

			var ev = ResolveEvent(node);

			if (ev == null)
				return null;

			try
			{
				var descriptor = ComponentDescriptor.DistributedEvent(new MicroServiceContext(Arguments.Editor.Context.MicroService.Token, Arguments.Editor.Context.Tenant.Url), ev);

				descriptor.Validate();
				descriptor.ValidateConfiguration();

				var e = descriptor.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

				if (e == null)
					return NoEvent();

				var type = Arguments.Editor.Context.Tenant.GetService<ICompilerService>().ResolveType(descriptor.MicroService.Token, e, e.Name, false);

				if (type == null)
					return NoParameters();

				var result = new List<ICompletionItem>();
				var existing = ExistingDeclarations(anon);
				var properties = new PropertyBrowser(type).Browse();

				foreach (var property in properties)
				{
					if (existing.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
						continue;

					var detail = property.PropertyType.ToFriendlyName();

					result.Add(new CompletionItem
					{
						FilterText = property.Name,
						Detail = detail,
						InsertText = property.Name,
						Label = property.Name,
						Kind = CompletionItemKind.Property,
						SortText = property.Name
					});
				}

				if (result.Count == 0)
					return AllSet();

				result.Add(BindProperties(properties, existing));

				return result;
			}
			catch
			{
				return null;
			}
		}

		private string ToPropertyName(string value)
		{
			var result = value;

			if (result.StartsWith("@"))
				result = result.Substring(1);

			return result.ToPascalCase();
		}

		private ICompletionItem BindProperties(List<System.Reflection.PropertyInfo> properties, List<string> existing)
		{
			var text = new StringBuilder();

			foreach (var property in properties)
			{
				if (existing.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
					continue;

				var value = TypeExtensions.DefaultValue(property.PropertyType);

				if (value == null)
				{
					var type = property.PropertyType;

					if (type == typeof(string))
						value = "string.Empty";
					else if (type == typeof(object))
						value = "new byte[]";
				}

				text.AppendLine($"{property.Name} = {value},");
			}

			return new CompletionItem
			{
				Detail = "Bind all properties that have not been set yet",
				InsertText = text.ToString(),
				Kind = CompletionItemKind.Snippet,
				Label = "Bind all properties",
				Preselect = true
			};
		}

		private string ResolveEvent(SyntaxNode node)
		{
			var argList = node.Closest<ArgumentListSyntax>();

			if (argList == null || argList.Arguments.Count == 0)
				return null;

			var argument = argList.Arguments[0];

			if (argument.Expression is LiteralExpressionSyntax literal)
			{
				var text = literal.GetText();

				if (text == null)
					return null;

				var result = text.ToString();

				if (result.StartsWith("\"") && result.EndsWith("\""))
					return result[1..^1];

				return result;
			}

			return null;
		}

		private List<ICompletionItem> NoEvent()
		{
			return new List<ICompletionItem>
			{
				new CompletionItem
				{
					Kind = CompletionItemKind.Text,
					Label="Not an event",
					Detail="Event does not belong to the middleware."
				}
			};
		}

		private List<ICompletionItem> NoParameters()
		{
			return new List<ICompletionItem>
			{
				new CompletionItem
				{
					Kind = CompletionItemKind.Text,
					Label="No properties",
					Detail="Distributed event does not have any properties."
				}
			};
		}

		private List<ICompletionItem> AllSet()
		{
			return new List<ICompletionItem>
			{
				new CompletionItem
				{
					Kind = CompletionItemKind.Text,
					Label="All properties set",
					Detail="All event properties are set."
				}
			};
		}

		private List<string> ExistingDeclarations(AnonymousObjectCreationExpressionSyntax syntax)
		{
			var result = new List<string>();

			foreach (var initializer in syntax.Initializers)
			{
				var text = initializer.NameEquals?.Name?.Identifier.ValueText;

				if (!string.IsNullOrWhiteSpace(text))
					result.Add(text);
			}

			return result;
		}
	}
}
