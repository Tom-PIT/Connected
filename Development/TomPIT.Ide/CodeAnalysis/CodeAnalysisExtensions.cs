using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomPIT.Ide.CodeAnalysis
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

		public static PropertyInfo ResolvePropertyInfo(this AssignmentExpressionSyntax assignment, SemanticModel model)
		{
			if (assignment.Left is MemberAccessExpressionSyntax ma)
				return ResolvePropertyInfo(ma, model);
			else if (assignment.Left is IdentifierNameSyntax ins)
				return ResolvePropertyInfo(ins, model);

			return null;
		}

		private static PropertyInfo ResolvePropertyInfo(IdentifierNameSyntax ins, SemanticModel model)
		{
			if (ins.Parent.Parent.Parent is ObjectCreationExpressionSyntax ocx)
			{
				var type = model.GetTypeInfo(ocx);
				var rtType = ResolveType(type);

				if(rtType != null)
					return rtType.GetProperty(ins.Identifier.ValueText);
			}

			return null;
		}

		private static PropertyInfo ResolvePropertyInfo(MemberAccessExpressionSyntax ma, SemanticModel model)
		{
			if(ma.Expression is IdentifierNameSyntax ins)
			{
				var scope = ins.DeclarationScope();

				if (scope == null)
					return null;

				var declaration = scope.VariableDeclaration(ins.Identifier.ValueText);

				if (declaration == null)
					return null;

				var type = model.GetTypeInfo(declaration.Initializer.Value);

				var rtType = ResolveType(type);

				if (rtType != null)
					return rtType.GetProperty(ma.Name.Identifier.ValueText);
			}

			return null;
		}

		public static Type ResolveType(this Microsoft.CodeAnalysis.TypeInfo typeInfo)
		{
			if (typeInfo.Type == null)
				return null;

			var sb = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(typeInfo.Type.ContainingNamespace.ToDisplayString()))
				sb.Append($"{typeInfo.Type.ContainingNamespace.ToDisplayString()}.");

			sb.Append($"{typeInfo.Type.Name}");

			if(!string.IsNullOrWhiteSpace(typeInfo.Type.ContainingAssembly.ToDisplayString()))
				sb.Append($", {typeInfo.Type.ContainingAssembly.ToDisplayString()}");

			return Types.GetType(sb.ToString());
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
	}
}