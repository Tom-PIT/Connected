using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TomPIT.Annotations.Design;
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
			if (context.Node is not CompilationUnitSyntax compilationUnit)
				return;

			CheckClassDeclaration(context);
		}

		private void CheckClassDeclaration(SyntaxNodeAnalysisContext context)
		{
			var expectedClassName = ResolveExpectedClassName();

			if (string.IsNullOrWhiteSpace(expectedClassName))
				return;

			var declarations = context.Node.ChildNodes();
			ClassDeclarationSyntax firstClass = null;

			foreach(var declaration in declarations)
			{
				if(declaration.IsKind(SyntaxKind.ClassDeclaration))
				{
					var cs = declaration as ClassDeclarationSyntax;

					if (string.Compare(cs.Identifier.ToString(), expectedClassName, false) == 0)
					{
						if(!cs.IsPublic())
						{
							var descriptor = new DiagnosticDescriptor("ComponentClass", "Invalid class modifier", "Script class should have public modifier.", "Naming", DiagnosticSeverity.Warning, true);
							var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(descriptor, cs.Identifier.GetLocation());
							
							context.ReportDiagnostic(diagnostic);
						}

						return;
					}

					if (firstClass == null)
						firstClass = cs;
				}
			}

			var descriptor1 = new DiagnosticDescriptor("ComponentClass", "Class not present", $"Script class should have class name '{expectedClassName}' with public modifier.", "Naming", DiagnosticSeverity.Warning, true);
			var diagnostic1 = Microsoft.CodeAnalysis.Diagnostic.Create(descriptor1, firstClass == null ? context.Node.GetLocation() : firstClass.Identifier.GetLocation());

			context.ReportDiagnostic(diagnostic1);
		}

		private string ResolveExpectedClassName()
		{
			var component = Tenant.GetService<IComponentService>().SelectComponent(Script.Component);

			if (component == null)
				return null;

			var configuration = Tenant.GetService<IComponentService>().SelectConfiguration(Script.Component);

			if (component.Token == Script.Id)
				return component.Name;

			var target = Tenant.GetService<Reflection.IDiscoveryService>().Find(configuration, Script.Id);

			if (target == null)
				return null;

			var att = target.GetType().FindAttribute<SyntaxAttribute>();

			if (att == null || string.IsNullOrWhiteSpace(att.ClassNameProperty))
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
						new DiagnosticDescriptor("ComponentClass", "Script should contain class with the same name as Component", null, "Naming", DiagnosticSeverity.Error, true)
					};
				}

				return _descriptors;
			}
		}
	}
}
