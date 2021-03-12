using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Reflection;

namespace TomPIT.Compilation.Analyzers
{
	[DiagnosticAnalyzer("csharp")]
	internal class ClassComplianceCodeAnalyer : DiagnosticAnalyzer
	{
		private List<DiagnosticDescriptor> _descriptors = null;
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Descriptors.ToImmutableArray();

		public ClassComplianceCodeAnalyer(ITenant tenant, IScriptDescriptor script)
		{
			Tenant = tenant;
			Script = script;
		}

		private IScriptDescriptor Script { get; }
		private ITenant Tenant { get; }

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.CompilationUnit);
		}

		private void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			if (context.Node is not CompilationUnitSyntax)
				return;

			CheckClassDeclaration(context);
		}

		private void CheckClassDeclaration(SyntaxNodeAnalysisContext context)
		{
			var expectedClassName = ResolveExpectedClassName();

			if (string.IsNullOrWhiteSpace(expectedClassName))
				return;

			var path = context.Node.GetLocation()?.GetLineSpan().Path;

			if (string.IsNullOrWhiteSpace(path) || path.Contains("/"))
				return;

			var declarations = context.Node.ChildNodes();
			ClassDeclarationSyntax firstClass = null;

			foreach (var declaration in declarations)
			{
				if (declaration.IsKind(SyntaxKind.ClassDeclaration))
				{
					var cs = declaration as ClassDeclarationSyntax;

					if (string.Compare(cs.Identifier.ToString(), expectedClassName, false) == 0)
					{
						if (!cs.IsPublic())
							context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(Descriptors[0], cs.Identifier.GetLocation()));

						return;
					}

					if (firstClass == null)
						firstClass = cs;
				}
			}

			var location = firstClass == null ? context.Node.GetLocation() : firstClass.Identifier.GetLocation();
			context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(Descriptors[1], location, expectedClassName));
		}

		private string ResolveExpectedClassName()
		{
			var component = Tenant.GetService<IComponentService>().SelectComponent(Script.Component);

			if (component == null)
				return null;

			var configuration = Tenant.GetService<IComponentService>().SelectConfiguration(Script.Component);

			var target = Tenant.GetService<IDiscoveryService>().Find(configuration, Script.Id);

			if (target == null)
				return null;

			var att = target.GetType().FindAttribute<ClassRequiredAttribute>();

			if (att == null)
				return null;

			if (component.Token == Script.Id)
				return component.Name;

			if (string.IsNullOrWhiteSpace(att.ClassNameProperty))
				return target.ToString();

			var property = target.GetType().GetProperty(att.ClassNameProperty);

			if (property == null)
				return null;

			return Types.Convert<string>(property.GetValue(target));
		}

		private List<DiagnosticDescriptor> Descriptors
		{
			get
			{
				if (_descriptors == null)
				{
					_descriptors = new List<DiagnosticDescriptor>
					{
						new DiagnosticDescriptor("ComponentClass", "Invalid class modifier", "Script class should have public modifier.", "Naming", DiagnosticSeverity.Warning, true),
						new DiagnosticDescriptor("ComponentClass", "Class not present", "Script should have class name '{0}' with public modifier.", "Naming", DiagnosticSeverity.Warning, true),
					};
				}

				return _descriptors;
			}
		}
	}
}
