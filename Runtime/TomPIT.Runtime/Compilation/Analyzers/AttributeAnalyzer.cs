using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TomPIT.Annotations.Design;
using TomPIT.Connectivity;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Reflection;

namespace TomPIT.Compilation.Analyzers
{
	internal class AttributeAnalyzer : AnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create
		(
			new DiagnosticDescriptor("TP3001", "Reference expected", "Reference expected in the format 'MicroService/Component'.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP3002", "Invalid reference", "Invalid reference. Expected 'MicroService/Component' format.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP3003", "Microservice not found", "Microservice {0} does not exist.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP3004", "Microservice reference required", "A reference to the Microservice {0} is required.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP3005", "Component not found", "Component {0} does not exist in Microservice {1}.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP3006", "Value expected", "Value expected from {0}.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP3007", "Value not found", "{0} '{1}' does not exist in the {2}.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP3008", "Element not found", "{0} {1} does not exist in Component {2}.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP3009", "Value not found", "'{0}' does not exist.", "Referencing", DiagnosticSeverity.Warning, true)
		);

		public AttributeAnalyzer(ITenant tenant, Guid microService, Guid component, Guid script) : base(tenant, microService, component, script)
		{
		}

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Argument);
			context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.AttributeArgument);
			context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleAssignmentExpression);
		}

		private void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			if (!IsScriptFile(context))
				return;

			AnalyzerAttribute providerAttribute = null;

			if (context.Node is AttributeArgumentSyntax att)
				providerAttribute = ResolveProvider(context, att);
			else if (context.Node is ArgumentSyntax arg)
				providerAttribute = ResolveProvider(context, arg);
			else if (context.Node is AssignmentExpressionSyntax asn)
				providerAttribute = ResolveProvider(context, asn);

			if (providerAttribute is null)
				return;

			var instance = Reflection.TypeExtensions.CreateInstance<IAttributeAnalyzer>(providerAttribute.Type is not null ? providerAttribute.Type : Reflection.TypeExtensions.GetType(providerAttribute.TypeName));

			if (instance is null)
				return;

			instance.Analyze(context, this);
		}

		private static AnalyzerAttribute ResolveProvider(SyntaxNodeAnalysisContext context, AssignmentExpressionSyntax syntax)
		{
			if (syntax.GetPropertySymbol(context.SemanticModel) is not IPropertySymbol property)
				return null;

			var target = property.GetAttributes().GetAttribute<AnalyzerAttribute>(context.SemanticModel);

			if (target is null)
				return null;

			return new AnalyzerAttribute(target.ConstructorArguments[0].Value as string);
		}

		private static AnalyzerAttribute ResolveProvider(SyntaxNodeAnalysisContext context, AttributeArgumentSyntax syntax)
		{
			if (syntax.GetArgumentList() is not AttributeArgumentListSyntax list)
				return null;

			if (list.GetMethodSymbol(context.SemanticModel)?.GetConstructorInfo(context.SemanticModel) is not ConstructorInfo ctor)
				return null;

			return ResolveProvider(list.Arguments.IndexOf(syntax), ctor);
		}

		private static AnalyzerAttribute ResolveProvider(SyntaxNodeAnalysisContext context, ArgumentSyntax syntax)
		{
			if (syntax.GetArgumentList() is not ArgumentListSyntax list)
				return null;

			if (list.GetMethodSymbol(context.SemanticModel)?.GetMethodInfo(context.SemanticModel) is not MethodInfo method)
				return null;

			return ResolveProvider(list.Arguments.IndexOf(syntax), method);
		}

		private static AnalyzerAttribute ResolveProvider(int index, MethodInfo method)
		{
			if (method.IsDefined(typeof(ExtensionAttribute)))
				index++;

			return ResolveParameter(index, method.GetParameters());
		}

		private static AnalyzerAttribute ResolveProvider(int index, ConstructorInfo ctor)
		{
			return ResolveParameter(index, ctor.GetParameters());
		}

		private static AnalyzerAttribute ResolveParameter(int index, ParameterInfo[] parameters)
		{
			if (parameters is null)
				return null;

			if (index >= parameters.Length)
				return null;

			return parameters[index].GetCustomAttribute<AnalyzerAttribute>();
		}

		private static IAttributeAnalyzer ResolveAttribute(AttributeData data)
		{
			if (data == null || data.ConstructorArguments.Length == 0)
				return null;

			var constant = data.ConstructorArguments[0];
			Type analyzerType = null;

			if (constant.Value is INamedTypeSymbol name)
				analyzerType = Reflection.TypeExtensions.GetType(name.ToDisplayName());
			else if (constant.Value is string cv)
				analyzerType = Reflection.TypeExtensions.GetType(cv);

			if (analyzerType is null)
				return null;

			return analyzerType.CreateInstance<IAttributeAnalyzer>();
		}
	}
}