using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data;
using TomPIT.Ide.TextServices.CSharp;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Reflection;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class ModelOperationParametersProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var node = Arguments.Model.SyntaxTree.GetRoot().FindNode(Editor.GetMappedSpan(Arguments.Position));

			if (!(node is ArgumentSyntax arg))
				return null;

			if (!(arg.Expression is AnonymousObjectCreationExpressionSyntax anon))
				return null;

			var descriptor = ResolveModel(node);

			if (descriptor == null)
				return null;

			var operation = ResolveOperation(node);

			if (operation == null)
				return null;

			try
			{
				descriptor.Validate();
				descriptor.ValidateConfiguration();

				if (descriptor.Configuration.Connection == Guid.Empty)
					return descriptor.Configuration.NoConnectionSet();

				var provider = Arguments.CreateOrmProvider(descriptor.Configuration);

				if (provider.Item1 == null)
					return descriptor.Configuration.NoOp();

				var op = descriptor.Configuration.Operations.FirstOrDefault(f => string.Compare(f.Name, operation, true) == 0);

				if (op == null)
					return NoOperation(descriptor.Configuration);

				var text = Arguments.Editor.Context.Tenant.GetService<IComponentService>().SelectText(descriptor.MicroService.Token, op);
				var commandDescriptor = provider.Item1.Parse(provider.Item2, new ModelOperationSchema { Text = text });

				if (commandDescriptor.Parameters.Count == 0)
					return NoParameters();

				var result = new List<ICompletionItem>();
				var existing = ExistingDeclarations(anon);

				foreach (var parameter in commandDescriptor.Parameters)
				{
					var name = ToPropertyName(parameter.Name);

					if (existing.Contains(name, StringComparer.OrdinalIgnoreCase))
						continue;

					var detail = Types.ToType(parameter.Type).ToFriendlyName();

					if (parameter.Value == DBNull.Value)
						detail += " (NULL)";
					else if (parameter.Value != null)
						detail += $" ({parameter.Value})";

					result.Add(new CompletionItem
					{
						FilterText = name,
						Detail = detail,
						InsertText = name,
						Label = name,
						Kind = CompletionItemKind.Property,
						SortText = name
					});
				}

				if (result.Count == 0)
					return AllSet();

				result.Add(BindParameters(commandDescriptor.Parameters, existing));

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

		private ICompletionItem BindParameters(List<ICommandTextParameter> parameters, List<string> existing)
		{
			var text = new StringBuilder();

			foreach (var parameter in parameters)
			{
				var name = ToPropertyName(parameter.Name);

				if (existing.Contains(name, StringComparer.OrdinalIgnoreCase))
					continue;

				object value;

				if (parameter.Value == DBNull.Value || parameter.Value == null)
					value = TypeExtensions.DefaultValue(Types.ToType(parameter.Type));
				else
					value = parameter.Value;

				if (value == null)
				{
					var type = Types.ToType(parameter.Type);

					if (type == typeof(string))
						value = "string.Empty";
					else if (type == typeof(object))
						value = "new byte[]";
				}

				text.AppendLine($"{name} = {value},");
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

		private string ResolveOperation(SyntaxNode node)
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

		private ConfigurationDescriptor<IModelConfiguration> ResolveModel(SyntaxNode node)
		{
			var invocation = node.Closest<InvocationExpressionSyntax>();

			if (invocation == null)
				return null;

			if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
				return null;

			var type = new TypeInfo();

			if (memberAccess.Expression is IdentifierNameSyntax ins)
				type = Arguments.Model.GetTypeInfo(ins.GetFirstToken().Parent);
			else if (memberAccess.Expression is InvocationExpressionSyntax ies)
				type = Arguments.Model.GetTypeInfo(memberAccess.Expression);

			if (type.Type == null || type.Type.DeclaringSyntaxReferences.Length == 0)
				return null;

			var sourceFile = type.Type.DeclaringSyntaxReferences[0].SyntaxTree.FilePath;

			if (string.IsNullOrWhiteSpace(sourceFile))
				sourceFile = $"{Arguments.Editor.Context.MicroService.Name}/{type.Type.Name}";

			sourceFile = Path.GetFileNameWithoutExtension(sourceFile);

			return ComponentDescriptor.Model(Arguments.Editor.Context, sourceFile);
		}

		private List<ICompletionItem> NoOperation(IModelConfiguration configuration)
		{
			return new List<ICompletionItem>
			{
				new CompletionItem
				{
					Kind = CompletionItemKind.Text,
					Label="Not an operation",
					Detail="Operation does not belong to the model."
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
					Detail="Operation does not have any properties."
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
					Detail="All operation properties are set."
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
