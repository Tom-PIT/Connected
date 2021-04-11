using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TomPIT.Connectivity;

namespace TomPIT.Compilation.Analyzers
{

	internal class ClassComplianceCodeAnalyer : AnalyzerBase
	{
		private List<DiagnosticDescriptor> _descriptors = null;
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Descriptors.ToImmutableArray();

		public ClassComplianceCodeAnalyer(ITenant tenant, Guid microService, Guid component, Guid script) : base(tenant, microService, component, script)
		{
		}

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.CompilationUnit);
		}

		private void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			if (!IsScriptFile(context))
				return;

			CheckClassDeclaration(context);
		}

		private void CheckClassDeclaration(SyntaxNodeAnalysisContext context)
		{
			var expectedClassName = ResolveExpectedClassName();

			if (string.IsNullOrWhiteSpace(expectedClassName))
				return;

			ClassDeclarationSyntax firstClass = null;

			foreach (var node in context.Node.DescendantNodes())
			{
				if (node is ClassDeclarationSyntax declaration)
				{
					if (firstClass is null)
						firstClass = declaration;

					if (string.Compare(declaration.Identifier.ValueText, expectedClassName, true) == 0)
					{
						if (!declaration.IsPublic())
							context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(Descriptors[0], declaration.Identifier.GetLocation()));

						return;
					}
				}
			}

			var location = firstClass == null ? context.Node.GetLocation() : firstClass.Identifier.GetLocation();

			context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(Descriptors[1], location, expectedClassName));
		}

		private List<DiagnosticDescriptor> Descriptors
		{
			get
			{
				if (_descriptors == null)
				{
					_descriptors = new List<DiagnosticDescriptor>
					{
						new DiagnosticDescriptor("TP1001", "Invalid class modifier", "Script class should have public modifier.", "Naming", DiagnosticSeverity.Warning, true),
						new DiagnosticDescriptor("TP1002", "Class not present", "Script should have class name '{0}' with public modifier.", "Naming", DiagnosticSeverity.Warning, true),
					};
				}

				return _descriptors;
			}
		}
	}
}
