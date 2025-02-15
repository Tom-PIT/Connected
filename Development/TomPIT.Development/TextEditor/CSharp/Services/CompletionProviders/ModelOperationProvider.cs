﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Ide.Analysis;
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

			var result = new List<ICompletionItem>();

			try
			{
				descriptor.Validate();
				descriptor.ValidateConfiguration();

				if (descriptor.Configuration.Connection == Guid.Empty)
					return descriptor.Configuration.NoConnectionSet();

				var provider = Arguments.CreateOrmProvider(descriptor.Configuration);

				if (provider.Item1 == null)
					return descriptor.Configuration.NoOp();

				foreach (var operation in descriptor.Configuration.Operations)
				{
					var text = Arguments.Editor.Context.Tenant.GetService<IComponentService>().SelectText(descriptor.MicroService.Token, operation);
					var type = OperationType.Other;

					try
					{
						type = provider.Item1.Parse(provider.Item2, new ModelOperationSchema { Text = text }).Statement;
					}
					catch
					{
						continue;
					}

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
			}
			catch(Exception ex)
			{
				result.Add(new CompletionItem
				{
					Detail = ex.Message,
					Kind = CompletionItemKind.Text,
					Label = ex.Source
				});
			}

			return result;
		}

		private ConfigurationDescriptor<IModelConfiguration> ResolveModel(SyntaxNode node)
		{
			var invocation = node.Closest<InvocationExpressionSyntax>();

			if (invocation == null)
				return null;

			ExpressionSyntax syntax = null;

			if (invocation.Expression is MemberAccessExpressionSyntax
				|| invocation.Expression is IdentifierNameSyntax)
				syntax = invocation.Expression;

			if (syntax == null)
				return null;

			var type = new TypeInfo();

			if (syntax is IdentifierNameSyntax ins)
				type = Arguments.Model.GetTypeInfo(ins.GetFirstToken().Parent);
			else if (syntax is InvocationExpressionSyntax)
				type = Arguments.Model.GetTypeInfo(syntax);
			else if (syntax is MemberAccessExpressionSyntax member)
				type = Arguments.Model.GetTypeInfo(member.Expression);

			if (type.Type == null || type.Type.DeclaringSyntaxReferences.Length == 0)
			{
				var scope = syntax.ClassDeclaration();

				if (scope == null)
					return null;

				var name = scope.Identifier.ToString();

				var model = ComponentDescriptor.Model(Arguments.Editor.Context, name);

				if (model.Component == null)
					return null;

				return model;
			}

			var sourceFile = type.Type.DeclaringSyntaxReferences[0].SyntaxTree.FilePath;

			if (string.IsNullOrWhiteSpace(sourceFile))
				sourceFile = $"{Arguments.Editor.Context.MicroService.Name}/{type.Type.Name}";

			sourceFile = Path.GetFileNameWithoutExtension(sourceFile);

			return ComponentDescriptor.Model(Arguments.Editor.Context, sourceFile);
		}
	}
}
