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
using TomPIT.Reflection.Api;

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

			var manifest = Editor.Context.Tenant.GetService<IDiscoveryService>().Manifests.Select(descriptor.Component.Token) as ApiManifest;

			if (manifest == null)
				return result;

			var operation = manifest.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			if (operation == null || string.IsNullOrWhiteSpace(operation.ReturnType))
				return result;

			var model = Editor.Document.GetSemanticModelAsync().Result;
			var caret = Editor.GetMappedCaret(Arguments.Position);
			var token = model.SyntaxTree.GetRoot().FindToken(caret);

			var stack = new Stack<string>();

			var node = token.Parent;

			if (node is IdentifierNameSyntax)
				node = node.Parent;

			CreatePropertyStack(model, node, stack);
			using var resolver = Editor.Context.Tenant.GetService<IDiscoveryService>().Manifests.SelectTypeResolver(operation);
			var manifestType = resolver.Resolve(operation.ReturnType);

			if (manifestType == null)
				return result;

			Dictionary<string, IManifestTypeDescriptor> properties = FindProperty(stack, manifestType.Members, resolver);

			if (properties == null)
				return result;

			foreach (var i in properties)
			{
				result.Add(new CompletionItem
				{
					Label = i.Key,
					Detail = i.Value.Name,
					FilterText = i.Key,
					Kind = CompletionItemKind.Property,
					SortText = i.Key,
					InsertText = i.Key
				});
			}

			return result;
		}

		private Dictionary<string, IManifestTypeDescriptor> FindProperty(Stack<string> stack, Dictionary<string, IManifestTypeDescriptor> properties, IManifestTypeResolver resolver)
		{
			if (stack.Count < 2)
				return properties;

			var result = properties;

			while (stack.Count > 0)
			{
				var current = stack.Pop();

				if (properties.FirstOrDefault(f => string.Compare(f.Key, current, true) == 0).Value is not IManifestTypeDescriptor property)
					return result;

				var type = resolver.Resolve(property.Name);

				if (type == null)
				{
					if (stack.Count > 0)
						return null;

					return result;
				}

				result = type.Members;
			}

			return result;
		}

		private void CreatePropertyStack(SemanticModel model, SyntaxNode node, Stack<string> stack)
		{
			if (node is MemberAccessExpressionSyntax member)
			{
				stack.Push(member.Name.Identifier.ValueText);

				CreatePropertyStack(model, member.Expression, stack);
			}
			else if (node is MemberBindingExpressionSyntax binding)
			{
				var previousToken = binding.OperatorToken.GetPreviousToken();

				stack.Push(binding.Name.Identifier.ValueText);

				CreatePropertyStack(model, previousToken.Parent, stack);
			}
			else if (node is ConditionalAccessExpressionSyntax conditional)
			{
				var previousToken = conditional.OperatorToken.GetPreviousToken();

				CreatePropertyStack(model, previousToken.Parent, stack);
			}
			else if (node is IdentifierNameSyntax identifier)
			{
				var token = model.SyntaxTree.GetRoot().FindToken(identifier.SpanStart);

				if (token.GetPreviousToken().Kind() == SyntaxKind.DotToken)
					stack.Push(identifier.Identifier.ValueText);
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
