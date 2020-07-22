using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Reflection;

namespace TomPIT.Ide.TextServices.CSharp
{
	public static class CSharpQuery
	{
		public static T Closest<T>(this SyntaxNode node) where T : SyntaxNode
		{
			if (node == null)
				return default;

			if (node is T)
				return (T)node;

			return Closest<T>(node.Parent);
		}
		public static bool IsInAttribute(this SyntaxNode node)
		{
			return node.Closest<AttributeSyntax>() != null;
		}

		public static string EnclosingAttributeName(this SyntaxNode node)
		{
			var syntax = node.Closest<AttributeSyntax>();

			if (syntax == null)
				return default;

			return !(syntax.Name is IdentifierNameSyntax name) ? null : name.Identifier.ValueText;
		}
		public static Microsoft.CodeAnalysis.TypeInfo ResolveMemberAccessTypeInfo(SemanticModel model, SyntaxNode node)
		{
			var member = node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();

			if (member == null)
				return default;

			if (member.Expression is IdentifierNameSyntax identifier)
				return model.GetTypeInfo(identifier);
			else if (member.Expression is InvocationExpressionSyntax ies)
				return model.GetTypeInfo(ies);
			else if (member.Expression is MemberAccessExpressionSyntax memberExpression)
				return ResolveMemberAccessTypeInfo(model, memberExpression);

			return default;
		}

		public static Microsoft.CodeAnalysis.TypeInfo ResolveTypeInfo(SemanticModel model, SyntaxNode node)
		{
			var type = model.GetSpeculativeTypeInfo(node.Span.Start, node, SpeculativeBindingOption.BindAsTypeOrNamespace);

			if (type.Type != null)
				return type;

			if (node is BaseTypeSyntax bts)
				return model.GetSpeculativeTypeInfo(node.Span.Start, bts.Type, SpeculativeBindingOption.BindAsTypeOrNamespace);

			return default;
		}

		public static SyntaxToken EnclosingIdentifier(SyntaxToken token)
		{
			var currenToken = token;

			while (true)
			{
				if (!currenToken.IsKind(SyntaxKind.IdentifierToken)
					&& !currenToken.IsKind(SyntaxKind.DotToken)
					&& !currenToken.IsKind(SyntaxKind.QuestionToken))
					break;

				currenToken = currenToken.GetPreviousToken();
			}

			if (currenToken.GetNextToken().IsKind(SyntaxKind.IdentifierToken))
				return currenToken.GetNextToken();

			return default;
		}

		public static bool IsAssignmentExpression(SyntaxNode node)
		{
			return node.Parent is AssignmentExpressionSyntax;
		}

		public static SymbolInfo GetInvocationSymbolInfo(SemanticModel model, AttributeArgumentListSyntax syntax)
		{
			if (syntax == null)
				return new SymbolInfo();

			if (!(syntax.Parent is AttributeSyntax invoke))
				return new SymbolInfo();

			return model.GetSymbolInfo(invoke);
		}

		public static SymbolInfo GetInvocationSymbolInfo(SemanticModel model, ArgumentListSyntax syntax)
		{
			if (syntax == null)
				return new SymbolInfo();

			if (syntax.Parent is InvocationExpressionSyntax invoke)
				return model.GetSymbolInfo(invoke);

			if ((syntax.Parent is ConstructorInitializerSyntax cinvoke))
				return model.GetSymbolInfo(cinvoke);

			return new SymbolInfo();
		}

		public static SymbolInfo GetInvocationSymbolInfo(SemanticModel model, ArgumentSyntax syntax)
		{
			return GetInvocationSymbolInfo(model, syntax.Parent as ArgumentListSyntax);
		}

		public static ArgumentListSyntax GetArgumentList(ArgumentSyntax syntax)
		{
			return syntax.Parent as ArgumentListSyntax;
		}

		public static AttributeArgumentListSyntax GetArgumentList(AttributeArgumentSyntax syntax)
		{
			return syntax.Parent as AttributeArgumentListSyntax;
		}

		public static MethodInfo GetMethodInfo(SemanticModel model, AttributeArgumentListSyntax syntax)
		{
			return GetMethodInfo(model, GetMethodSymbol(model, syntax));
		}

		public static MethodInfo GetMethodInfo(SemanticModel model, ArgumentListSyntax syntax)
		{
			return GetMethodInfo(model, GetMethodSymbol(model, syntax));
		}

		public static IMethodSymbol GetMethodSymbol(SemanticModel model, ArgumentSyntax syntax)
		{
			return GetMethodSymbol(model, syntax.Parent as ArgumentListSyntax);
		}
		public static MethodInfo GetMethodInfo(SemanticModel model, ArgumentSyntax syntax)
		{
			return GetMethodInfo(model, GetArgumentList(syntax));
		}
		public static MethodInfo GetMethodInfo(SemanticModel model, IMethodSymbol ms)
		{
			if (ms == null)
				return null;

			if (ms.IsExtensionMethod)
				ms = ms.GetConstructedReducedFrom();

			if (ms == null)
				return null;

			var type = Type.GetType(ms.ContainingType.TypeIdentifier());

			if (type == null)
				return null;

			var methodName = ms.Name;

			var methodArgumentTypeNames = new List<string>();

			foreach (var i in ms.Parameters)
			{
				if (i.Type.ContainingNamespace == null || i.Type.ContainingAssembly == null)
					continue;

				var argumentType = i.Type.TypeIdentifier();

				if (!string.IsNullOrWhiteSpace(argumentType))
					methodArgumentTypeNames.Add(argumentType);
			}

			var argumentTypes = methodArgumentTypeNames.Select(typeName => Type.GetType(typeName));
			var methods = type.GetAllMethods();

			foreach (var method in methods)
			{
				if (string.Compare(method.Name, ms.Name, false) != 0)
					continue;

				if (ms.TypeParameters != null && method.GetGenericArguments().Count() != ms.TypeParameters.Length)
					continue;

				var parameters = method.GetParameters().ToList();

				if (parameters.Count() != ms.Parameters.Length)
					continue;

				bool match = true;

				for (var i = 0; i < methodArgumentTypeNames.Count; i++)
				{
					var at = Reflection.TypeExtensions.GetType(methodArgumentTypeNames[i]);

					if (at == null)
						continue;

					var pt = parameters[i].ParameterType;

					if (pt.IsGenericMethodParameter)
						continue;

					if (at.IsGenericType && pt.IsGenericType)
					{
						var genericType = at.MakeGenericType(pt.GetGenericArguments());

						if (genericType == pt)
							continue;
					}

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

		public static IMethodSymbol GetMethodSymbol(SemanticModel model, AttributeArgumentListSyntax syntax)
		{
			var si = GetInvocationSymbolInfo(model, syntax);

			if (si.Symbol == null && si.CandidateSymbols.Length == 0)
				return null;

			return si.Symbol == null
				? si.CandidateSymbols[0] as IMethodSymbol
				: si.Symbol as IMethodSymbol;
		}

		public static IMethodSymbol GetMethodSymbol(SemanticModel model, ArgumentListSyntax syntax)
		{
			var si = GetInvocationSymbolInfo(model, syntax);

			if (si.Symbol != null)
				return si.Symbol as IMethodSymbol;

			if (si.CandidateSymbols.Length == 0)
				return null;

			if (syntax.Arguments != null && syntax.Arguments.Count > 0)
			{
				foreach (var candidate in si.CandidateSymbols)
				{
					if (!(candidate is IMethodSymbol methodSymbol))
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

			return si.CandidateSymbols[0] as IMethodSymbol;
		}

		private static bool Compare(ExpressionSyntax syntax, IParameterSymbol parameter)
		{
			if (parameter.Type == null)
				return false;

			if (string.Compare(parameter.ToDisplayString(), "string", true) == 0 && syntax.IsKind(SyntaxKind.StringLiteralExpression))
				return true;

			return false;
		}

		public static ConstructorInfo GetConstructorInfo(SemanticModel model, IMethodSymbol ms)
		{
			if (ms == null)
				return null;

			if (ms.IsExtensionMethod)
				ms = ms.GetConstructedReducedFrom();

			if (ms == null)
				return null;

			var declaringTypeName = string.Format(
				"{0}.{1}, {2}",
				ms.ContainingType.ContainingNamespace.ToString(),
				ms.ContainingType.MetadataName,
				ms.ContainingAssembly.ToDisplayString()
			);

			var type = Type.GetType(declaringTypeName);

			if (type == null)
				return null;

			var methodName = ms.Name;
			var methodArgumentTypeNames = new List<string>();

			foreach (var i in ms.Parameters)
			{
				if (i.Type.ContainingNamespace == null || i.Type.ContainingAssembly == null)
					continue;

				methodArgumentTypeNames.Add(string.Format("{0}.{1}, {2}", i.Type.ContainingNamespace.ToString(), i.Type.MetadataName, i.Type.ContainingAssembly.ToDisplayString()));
			}

			var argumentTypes = methodArgumentTypeNames.Select(typeName => Type.GetType(typeName));
			var constructors = type.GetAllConstructors();

			foreach (var ctor in constructors)
			{
				if (ctor.GetParameters().Count() != ms.Parameters.Length)
					continue;

				bool match = true;
				var parameters = ctor.GetParameters();

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
					return ctor;
			}

			return null;
		}

		public static MarkerSeverity ToMarkerSeverity(this DiagnosticSeverity severity)
		{
			return severity switch
			{
				DiagnosticSeverity.Hidden => MarkerSeverity.Hint,
				DiagnosticSeverity.Info => MarkerSeverity.Info,
				DiagnosticSeverity.Warning => MarkerSeverity.Warning,
				DiagnosticSeverity.Error => MarkerSeverity.Error,
				_ => MarkerSeverity.Info,
			};
		}

		public static List<MethodInfo> GetAllMethods(this Type type)
		{
			var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(f => f.MemberType == MemberTypes.Method);
			var result = new List<MethodInfo>();

			foreach (var member in members)
			{
				if (member is MethodInfo method)
					result.Add(method);
			}

			return result;
		}

		public static List<ConstructorInfo> GetAllConstructors(this Type type)
		{
			var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(f => f.MemberType == MemberTypes.Constructor);
			var result = new List<ConstructorInfo>();

			foreach (var member in members)
			{
				if (member is ConstructorInfo constructor)
					result.Add(constructor);
			}

			return result;
		}

		public static string TypeIdentifier(this ITypeSymbol symbol)
		{
			if (symbol == null)
				return null;

			var ns = symbol.ContainingNamespace == null ? string.Empty : $"{symbol.ContainingNamespace.ToString()}.";

			return $"{ns}{symbol.MetadataName}, {symbol.ContainingAssembly.ToDisplayString()}";
		}

		public static IdentifierNameSyntax IdentiferName(this InvocationExpressionSyntax syntax)
		{
			if (syntax == null)
				return null;

			var current = syntax.Expression;

			while (current != null)
			{
				if (current is IdentifierNameSyntax idn)
					return idn;

				if (current is MemberAccessExpressionSyntax ma)
					current = ma.Expression;
				else
					break;
			}

			return null;
		}
	}
}
