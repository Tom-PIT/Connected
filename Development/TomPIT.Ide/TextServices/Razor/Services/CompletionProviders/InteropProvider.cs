using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.TextServices.CSharp;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Reflection.Manifests.Entities;

namespace TomPIT.Ide.TextServices.Razor.Services.CompletionProviders
{
	internal class InteropProvider : CompletionProvider
	{
		public override bool WillProvideItems(CompletionProviderArgs e, IReadOnlyCollection<ICompletionItem> existing)
		{
			return existing.Count == 0;
		}

		protected override List<ICompletionItem> OnProvideItems()
		{
			var result = new List<ICompletionItem>();
			var descriptor = ResolveApi();

			if (descriptor == null)
				return result;

			var op = descriptor.Configuration.Operations.FirstOrDefault(f => string.Compare(descriptor.Element, f.Name, true) == 0);

			if (op == null)
				return result;

			var manifest = Editor.Context.Tenant.GetService<IDiscoveryService>().Manifest(descriptor.Component.Token, op.Id) as ApiManifest;

			if (manifest == null)
				return result;

			var operation = manifest.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			if (operation == null || operation.ReturnType == null)
				return result;

			var model = Editor.Document.GetSemanticModelAsync().Result;
			var caret = Editor.GetMappedCaret(Arguments.Position);
			var token = model.SyntaxTree.GetRoot().FindToken(caret);

			var stack = new Stack<string>();

			var node = token.Parent;

			if (node is IdentifierNameSyntax)
				node = node.Parent;

			CreatePropertyStack(node, stack);
			var properties = FindProperty(stack, operation.ReturnType.Properties, manifest.Types);

			foreach (var i in properties)
			{
				result.Add(new CompletionItem
				{
					Label = i.Name,
					Detail = i.Type,
					FilterText = i.Name,
					Kind = CompletionItemKind.Property,
					SortText = i.Name,
					InsertText = i.Name
				});
			}

			return result;
		}

		private List<ManifestProperty> FindProperty(Stack<string> stack, List<ManifestProperty> properties, List<ManifestMember> types)
		{
			if (stack.Count == 0)
				return properties;

			var result = properties;

			while (stack.Count > 0)
			{
				var current = stack.Pop();

				var property = properties.FirstOrDefault(f => string.Compare(f.Name, current, true) == 0);

				if (property == null)
					return result;

				var type = types.FirstOrDefault(f => string.Compare(property.Type, f.Type, false) == 0);

				if (type == null)
					return result;

				result = type.Properties;
			}

			return result;
		}

		private void CreatePropertyStack(SyntaxNode node, Stack<string> stack)
		{
			if (node is MemberAccessExpressionSyntax member)
			{
				stack.Push(member.Name.Identifier.ValueText);

				CreatePropertyStack(member.Expression, stack);
			}
		}
		private ConfigurationDescriptor<IApiConfiguration> ResolveApi()
		{
			var model = Editor.Document.GetSemanticModelAsync().Result;
			var caret = Editor.GetMappedCaret(Arguments.Position);

			if (caret == -1)
				return default;

			var token = model.SyntaxTree.GetRoot().FindToken(caret);

			if (token == default)
				return default;

			var identifier = CSharpQuery.EnclosingIdentifier(token);

			if (identifier == default)
				return default;

			var invocation = ResolveInvocationExpression(model, identifier);

			if (invocation == null)
				return default;

			var symbol = model.GetSymbolInfo(invocation);

			if (symbol.Symbol == null || !(symbol.Symbol is IMethodSymbol method))
				return default;

			if (string.Compare(method.ContainingType.ToDisplayName(), typeof(IMiddlewareInterop).FullTypeName(), false) != 0)
				return null;

			if (invocation.ArgumentList.Arguments.Count == 0)
				return default;

			var api = invocation.ArgumentList.Arguments[0].Expression.GetText().ToString().Trim('"');
			var descriptor = ComponentDescriptor.Api(Editor.Context, api);

			try
			{
				descriptor.Validate();
			}
			catch
			{
				return default;
			}

			return descriptor;
		}

		private InvocationExpressionSyntax ResolveInvocationExpression(SemanticModel model, SyntaxToken token)
		{
			if (!(token.Parent is IdentifierNameSyntax identifier))
				return null;

			var variable = FindVariableDeclaration(token);

			if (variable == null)
				return null;

			var type = model.GetTypeInfo(variable.Declaration.Type);

			if (type.Type == null || type.Type.TypeKind != TypeKind.Dynamic)
				return null;

			var assignment = FindLastAssignment(token);

			if (assignment == null)
			{
				foreach (var variableDeclaration in variable.Declaration.Variables)
				{
					if (string.Compare(variableDeclaration.Identifier.ValueText, identifier.Identifier.ValueText, true) == 0)
						return variableDeclaration.Initializer.Value as InvocationExpressionSyntax;
				}
			}
			else
			{
				if (assignment.Expression is AssignmentExpressionSyntax ase)
					return ase.Right as InvocationExpressionSyntax;
			}

			return null;
		}
		private ExpressionStatementSyntax FindLastAssignment(SyntaxToken token)
		{
			if (!(token.Parent is IdentifierNameSyntax identifier))
				return null;

			var result = new List<CSharpSyntaxNode>();
			SyntaxNode current = identifier;

			while (result.Count == 0)
			{
				var block = current.Closest<BlockSyntax>();

				if (block == null)
					break;

				var expressions = block.DescendantNodes().OfType<ExpressionStatementSyntax>();

				for (var i = expressions.Count() - 1; i >= 0; i--)
				{
					var expression = expressions.ElementAt(i);

					if (!(expression.Expression is AssignmentExpressionSyntax assign))
						continue;

					if (!(assign.Left is IdentifierNameSyntax identifierName))
						continue;

					if (string.Compare(identifierName.Identifier.ValueText, identifier.Identifier.ValueText, false) == 0)
						return expression;
				}

				current = block.Parent;

				if (current == null)
					break;
			}

			return null;
		}

		private LocalDeclarationStatementSyntax FindVariableDeclaration(SyntaxToken token)
		{
			if (!(token.Parent is IdentifierNameSyntax identifier))
				return null;

			var result = new List<CSharpSyntaxNode>();
			SyntaxNode current = identifier;

			while (result.Count == 0)
			{
				var block = current.Closest<BlockSyntax>();

				if (block == null)
					break;

				var declarations = block.ChildNodes().OfType<LocalDeclarationStatementSyntax>();

				foreach (var declaration in declarations)
				{
					foreach (var variableDeclaration in declaration.Declaration.Variables)
					{
						if (string.Compare(variableDeclaration.Identifier.ValueText, identifier.Identifier.ValueText, true) == 0)
						{
							return declaration;
						}
					}
				}

				current = block.Parent;

				if (current == null)
					break;
			}

			return null;
		}
	}
}
