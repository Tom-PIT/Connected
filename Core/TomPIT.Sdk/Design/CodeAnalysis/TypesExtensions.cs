using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Reflection;

namespace TomPIT.Design.CodeAnalysis
{
	public static class TypesExtensions
	{
		public static ITypeSymbol LookupBaseType(this ClassDeclarationSyntax scope, SemanticModel model, string baseTypeName)
		{
			if (scope.BaseList == null)
				return null;

			foreach (var baseType in scope.BaseList.Types)
			{
				var r = model.GetTypeInfo(baseType.Type).LookupBaseType(model, baseTypeName);

				if (r != null)
					return r;
			}

			return null;
		}

		public static ITypeSymbol LookupBaseType(this TypeInfo type, SemanticModel model, string baseTypeName)
		{
			if (type.Type == null)
				return null;

			return type.Type.LookupBaseType(model, baseTypeName);
		}

		public static ITypeSymbol LookupBaseType(INamedTypeSymbol type, SemanticModel model, string baseTypeName)
		{
			if (type == null)
				return null;

			var displayName = type.ToDisplayString();

			if (string.Compare(displayName, baseTypeName, false) == 0)
				return type;

			foreach (var itf in type.AllInterfaces)
			{
				displayName = itf.ToDisplayString();

				if (string.Compare(displayName, baseTypeName, false) == 0)
					return itf;
			}

			if (type.BaseType == null)
				return null;

			return LookupBaseType(type.BaseType, model, baseTypeName);
		}

		public static ITypeSymbol LookupBaseType(this ITypeSymbol type, SemanticModel model, string baseTypeName)
		{
			if (type == null)
				return null;

			var displayName = type.ToDisplayName();

			if (string.Compare(displayName, baseTypeName, false) == 0)
				return type;

			foreach (var itf in type.AllInterfaces)
			{
				displayName = itf.ToDisplayName();

				if (string.Compare(displayName, baseTypeName, false) == 0)
					return itf;
			}

			if (type.BaseType == null)
				return null;

			return type.BaseType.LookupBaseType(model, baseTypeName);
		}

		public static bool IsInheritedFrom(this ITypeSymbol symbol, string type)
		{
			var current = symbol;

			while (current != null)
			{
				if (string.Compare(current.ToDisplayString(), type, false) == 0)
					return true;

				current = current.BaseType;
			}

			return false;
		}

		public static bool IsOfType(this INamedTypeSymbol symbol, Type type)
		{
			return string.Compare(CodeAnalysisExtentions.ToDisplayName(symbol), type.FullTypeName(), false) == 0;
		}

		public static bool IsOfType(this TypeInfo symbol, Type type)
		{
			if (symbol.ConvertedType == null)
				return false;

			return string.Compare(CodeAnalysisExtentions.ToDisplayName(symbol.ConvertedType), type.FullTypeName(), false) == 0;
		}
	}
}
