using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Reflection;

namespace TomPIT.Design.CodeAnalysis
{
	public static class MethodExtensions
	{
		public static IMethodSymbol GetMethodSymbol(this AttributeArgumentListSyntax syntax, SemanticModel model)
		{
			var si = syntax.GetInvocationSymbolInfo(model);

			if (si.Symbol is null && !si.CandidateSymbols.Any())
				return null;

			return si.Symbol is null
				? si.CandidateSymbols[0] as IMethodSymbol
				: si.Symbol as IMethodSymbol;
		}

		public static IMethodSymbol GetMethodSymbol(this ArgumentListSyntax syntax, SemanticModel model)
		{
			if (syntax.GetInvocationSymbolInfo(model) is SymbolInfo symbol && symbol.Symbol is not null)
				return symbol.Symbol as IMethodSymbol;

			if (!symbol.CandidateSymbols.Any())
				return null;

			if (syntax.Arguments.Any())
			{
				foreach (var candidate in symbol.CandidateSymbols)
				{
					if (candidate is not IMethodSymbol methodSymbol)
						continue;

					var match = true;

					if (methodSymbol.Parameters.Length < syntax.Arguments.Count)
						continue;

					for (var i = 0; i < syntax.Arguments.Count; i++)
					{
						var argument = syntax.Arguments[i];

						if (!Compare(argument.Expression, methodSymbol.Parameters[i]))
						{
							match = false;
							break;
						}
					}

					if (match)
						return methodSymbol;
				}
			}

			return symbol.CandidateSymbols.First() as IMethodSymbol;
		}

		public static SymbolInfo GetInvocationSymbolInfo(this BaseArgumentListSyntax syntax, SemanticModel model)
		{
			if (syntax is null)
				return new SymbolInfo();

			if (syntax.Parent is AttributeSyntax invoke)
				return model.GetSymbolInfo(invoke);
			else if (syntax.Parent is InvocationExpressionSyntax invocation)
				return model.GetSymbolInfo(invocation);

			return new SymbolInfo();
		}

		public static SymbolInfo GetInvocationSymbolInfo(this AttributeArgumentListSyntax syntax, SemanticModel model)
		{
			if (syntax is null)
				return new SymbolInfo();

			if (syntax.Parent is AttributeSyntax invoke)
				return model.GetSymbolInfo(invoke);
			else if (syntax.Parent is InvocationExpressionSyntax invocation)
				return model.GetSymbolInfo(invocation);

			return new SymbolInfo();
		}

		private static bool Compare(ExpressionSyntax syntax, IParameterSymbol parameter)
		{
			if (parameter.Type == null)
				return false;

			if (string.Compare(parameter.ToDisplayString(), "string", true) == 0 && syntax.IsKind(SyntaxKind.StringLiteralExpression))
				return true;

			return false;
		}

		public static MethodInfo GetMethodInfo(this IMethodSymbol ms, SemanticModel model)
		{
			if (ms is null)
				return null;

			if (ms.IsExtensionMethod)
				ms = ms.GetConstructedReducedFrom();

			if (ms is null)
				return null;

			if (Type.GetType(ms.DeclaringTypeName()) is not Type type)
				return null;

			var methodName = ms.Name;
			var methodArgumentTypeNames = new List<string>();

			foreach (var i in ms.Parameters)
			{
				if (i.Type.DeclaringTypeName() is string declaringType)
					methodArgumentTypeNames.Add(declaringType);
			}

			var argumentTypes = methodArgumentTypeNames.Select(typeName => Type.GetType(typeName));
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);

			foreach (var method in methods)
			{
				if (string.Compare(method.Name, ms.Name, false) != 0)
					continue;

				if (method.GetParameters().Length != ms.Parameters.Length)
					continue;

				bool match = true;
				var parameters = method.GetParameters();

				for (var i = 0; i < methodArgumentTypeNames.Count; i++)
				{
					var at = Reflection.TypeExtensions.GetType(methodArgumentTypeNames[i]);

					if (at == null)
						continue;

					var pt = parameters[i].ParameterType;

					if (pt.IsGenericMethodParameter)
						continue;

					if (pt.IsInterface)
					{
						if (at != pt && !at.ImplementsInterface(pt))
						{
							match = false;
							break;
						}
					}
					else if (at != pt && !at.IsSubclassOf(pt))
					{
						match = false;
						break;
					}
				}

				if (match)
					return method;
			}

			return null;
		}
	}
}
