using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Reflection;

namespace TomPIT.Ide.Analysis
{
	public static class CodeAnalysisExtensions
	{
		public static MemberDeclarationSyntax DeclarationScope(this SyntaxNode node)
		{
			var current = node;

			while (current != null)
			{
				if (current is MemberDeclarationSyntax md)
					return md;

				current = current.Parent;
			}

			return null;
		}

		public static VariableDeclaratorSyntax VariableDeclaration(this MemberDeclarationSyntax declaration, string variableName)
		{
			var declarations = declaration.DescendantNodes().OfType<VariableDeclarationSyntax>();

			foreach (var dc in declarations)
			{
				var target = dc.Variables.FirstOrDefault(f => string.Compare(variableName, f.Identifier.Text, false) == 0);

				if (target != null)
					return target;
			}

			return null;
		}

		public static List<AttributeData> FindAttributes(this ISymbol symbol, string typeName)
		{
			var attributes = symbol.GetAttributes();
			var result = new List<AttributeData>();

			foreach (var attribute in attributes)
			{
				if (string.Compare(attribute.AttributeClass.ToDisplayName(), typeName, true) == 0)
					result.Add(attribute);
			}

			return result;
		}

		public static AttributeData FindAttribute(this ISymbol symbol, string typeName)
		{
			var attributes = symbol.GetAttributes();

			foreach (var attribute in attributes)
			{
				if (string.Compare(attribute.AttributeClass.ToDisplayName(), typeName, true) == 0)
					return attribute;
			}

			return default;
		}

		public static string ToDisplayName(this INamedTypeSymbol symbol)
		{
			var sb = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(symbol.ContainingNamespace.Name))
				sb.Append($"{symbol.ContainingNamespace.ToDisplayString()}.");

			sb.Append(symbol.MetadataName);

			if (!string.IsNullOrWhiteSpace(symbol.ContainingAssembly.Name))
				sb.Append($", {symbol.ContainingAssembly.ToDisplayString()}");

			return sb.ToString();
		}

		public static ISymbol ResolvePropertyInfo(this AssignmentExpressionSyntax assignment, SemanticModel model)
		{
			if (assignment.Left is MemberAccessExpressionSyntax ma)
				return ResolvePropertyInfo(ma, model);
			else if (assignment.Left is IdentifierNameSyntax ins)
				return ResolvePropertyInfo(ins, model);

			return null;
		}

		private static ISymbol ResolvePropertyInfo(IdentifierNameSyntax ins, SemanticModel model)
		{
			if (ins.Parent.Parent.Parent is ObjectCreationExpressionSyntax ocx)
			{
				var type = model.GetTypeInfo(ocx);

				return ResolveProperty(type.Type, ins.Identifier.ValueText);
			}

			return null;
		}

		private static ISymbol ResolveProperty(ITypeSymbol symbol, string propertyName)
		{
			if (symbol == null)
				return null;

			var members = symbol.GetMembers(propertyName);

			if (members == ImmutableArray<ISymbol>.Empty)
				return ResolveProperty(symbol.BaseType, propertyName);

			foreach (var i in members)
			{
				if (i.Kind == SymbolKind.Property)
					return i;
			}

			return ResolveProperty(symbol.BaseType, propertyName);
		}

		private static ISymbol ResolvePropertyInfo(MemberAccessExpressionSyntax ma, SemanticModel model)
		{
			if (ma.Expression is IdentifierNameSyntax ins)
			{
				var scope = ins.DeclarationScope();

				if (scope == null)
					return null;

				var declaration = scope.VariableDeclaration(ins.Identifier.ValueText);

				if (declaration == null)
					return null;

				var type = model.GetTypeInfo(declaration.Initializer.Value);

				ResolveProperty(type.ConvertedType, ma.Name.Identifier.ValueText);
			}

			return null;
		}

		public static Type ResolveType(this Microsoft.CodeAnalysis.TypeInfo typeInfo)
		{
			if (typeInfo.Type == null)
				return null;

			var sb = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(typeInfo.Type.ContainingNamespace.Name))
				sb.Append($"{typeInfo.Type.ContainingNamespace.ToDisplayString()}.");

			sb.Append($"{typeInfo.Type.Name}");

			if (!string.IsNullOrWhiteSpace(typeInfo.Type.ContainingAssembly.ToDisplayString()))
				sb.Append($", {typeInfo.Type.ContainingAssembly.ToDisplayString()}");

			return Reflection.TypeExtensions.GetType(sb.ToString());
		}

		public static string RenderValue(Type type)
		{
			if (type == typeof(string))
				return $"string.Empty";
			else if (type == typeof(int))
				return "0";
			else if (type == typeof(Guid))
				return "Guid.Empty";
			else if (type == typeof(DateTime))
				return "DateTime.MinValue";
			else
				return "null";
		}

		internal static ICompletionProvider ResolveCompletionProvider(AttributeData data)
		{
			if (data == null || data.ConstructorArguments.Length == 0)
				return null;

			var constant = data.ConstructorArguments[0];
			Type completionType = null;

			if (constant.Value is INamedTypeSymbol name)
				completionType = Reflection.TypeExtensions.GetType(name.ToDisplayName());
			else if (constant.Value is string cv)
				completionType = Reflection.TypeExtensions.GetType(cv);

			if (completionType == null)
				return null;

			return completionType.CreateInstance<ICompletionProvider>();
		}
	}
}