using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Ide.TextServices.CSharp;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal abstract class ModelOperationProvider : CompletionProvider
	{
		protected ModelOperationProvider(bool query)
		{
			Query = query;
		}

		private bool Query { get; }
		protected override List<ICompletionItem> OnProvideItems()
		{
			var node = Arguments.Model.SyntaxTree.GetRoot().FindNode(Editor.GetMappedSpan(Arguments.Position));
			var descriptor = ResolveModel(node);

			if (descriptor == null)
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

				var result = new List<ICompletionItem>();

				foreach (var operation in descriptor.Configuration.Operations)
				{
					var text = Arguments.Editor.Context.Tenant.GetService<IComponentService>().SelectText(descriptor.MicroService.Token, operation);
					var type = provider.Item1.Parse(provider.Item2, new ModelOperationSchema { Text = text }).Statement;

					if ((Query && (type == OperationType.Query || type == OperationType.Select))
						|| (!Query && (type == OperationType.Delete || type == OperationType.Insert || type == OperationType.Update))
						|| type == OperationType.Other)
					{
						result.Add(new CompletionItem
						{
							Detail = text.Length < 256 ? text : text.Substring(0, 256),
							FilterText = operation.Name,
							InsertText = operation.Name,
							Kind = CompletionItemKind.Reference,
							Label = operation.Name,
							SortText = operation.Name
						});
					}
				}

				return result;
			}
			catch
			{
				return null;
			}
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
	}
}
