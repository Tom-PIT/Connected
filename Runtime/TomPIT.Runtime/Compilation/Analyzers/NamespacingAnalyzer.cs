using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Reflection;

namespace TomPIT.Compilation.Analyzers
{
	internal class NamespacingAnalyzer : AnalyzerBase
	{
		private List<DiagnosticDescriptor> _descriptors = null;
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Descriptors.ToImmutableArray();

		public NamespacingAnalyzer(ITenant tenant, Guid microService, Guid component, Guid script) : base(tenant, microService, component, script)
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

			CheckNamespace(context);
		}

		private void CheckNamespace(SyntaxNodeAnalysisContext context)
		{
			var ns = ResolveNamespace();

			if (ns.Item1 && ns.Item2 is null)
			{
				ReportNamespaceExpected(context);
				return;
			}

			if (ns.Item2 is null)
				return;

			var queue = new Queue<string>(ns.Item2.Namespace.Split('.'));
			var result = ProcessPartial(context.Node, queue);

			if (result.Item1)
				return;

			var location = result.Item2 is null ? context.Node.GetLocation() : result.Item2.Identifier.GetLocation();

			context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(Descriptors[0], location, ns.Item2.Namespace));
		}

		private void ReportNamespaceExpected(SyntaxNodeAnalysisContext context)
		{
			ClassDeclarationSyntax cd = null;

			foreach (var child in context.Node.DescendantNodes())
			{
				if (child is ClassDeclarationSyntax declaration)
				{
					cd = declaration;
					break;
				}
			}

			var location = cd is null ? context.Node.GetLocation() : cd.Identifier.GetLocation();

			context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(Descriptors[1], location));
		}

		private (bool, ClassDeclarationSyntax) ProcessPartial(SyntaxNode node, Queue<string> queue)
		{
			var current = queue.Dequeue();
			ClassDeclarationSyntax cd = null;

			foreach (var child in node.DescendantNodes())
			{
				if (child is ClassDeclarationSyntax declaration)
				{
					if (ClassExtensions.IsPlatformClass(declaration.Identifier.ValueText))
						continue;

					cd = declaration;

					if (declaration.IsPartial())
					{
						if (string.Compare(declaration.Identifier.ValueText, current, true) == 0)
						{
							if (queue.IsEmpty())
								return (true, null);

							return ProcessPartial(declaration, queue);
						}
					}
				}
			}

			return (false, cd);
		}
		private List<DiagnosticDescriptor> Descriptors
		{
			get
			{
				if (_descriptors == null)
				{
					_descriptors = new List<DiagnosticDescriptor>
					{
						new DiagnosticDescriptor("TP2001", "Scoping expected", "Script classes should be scoped as '{0}' in partial classes.", "Naming", DiagnosticSeverity.Warning, true),
						new DiagnosticDescriptor("TP2002", "Namespace expected", "Namespace is expected on this component.", "Naming", DiagnosticSeverity.Warning, true),
					};
				}

				return _descriptors;
			}
		}

		private (bool, INamespaceElement) ResolveNamespace()
		{
			if (Text is null)
				return (false, null);

			var hasNamespace = false;

			if (Text is INamespaceElement nse)
			{
				hasNamespace = true;

				if (!string.IsNullOrWhiteSpace(nse.Namespace))
					return (true, nse);
			}

			IElement current = Text;

			while (current is not null)
			{
				current = current.Parent?.Closest<INamespaceElement>();

				if (current is null)
					break;

				hasNamespace = true;

				if (!string.IsNullOrWhiteSpace(((INamespaceElement)current).Namespace))
					return (hasNamespace, current as INamespaceElement);
			}

			return (hasNamespace, null);
		}
	}
}
