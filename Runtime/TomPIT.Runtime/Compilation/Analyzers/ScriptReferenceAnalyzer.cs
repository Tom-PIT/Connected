using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Reflection;

namespace TomPIT.Compilation.Analyzers
{
	internal class ScriptReferenceAnalyzer : AnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create
		(
			new DiagnosticDescriptor("TP4001", "Obsolete reference", "Reference should always contain Microservice. Support for references without Microservices will be removed in the future.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP4002", "Microservice not found", "Microservice {0} does not exist.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP4003", "Microservice reference required", "A reference to the Microservice {0} is required.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP4004", "Component not found", "Component {0} doesn't exist in Microservice {1}.", "Referencing", DiagnosticSeverity.Warning, true),
			new DiagnosticDescriptor("TP4005", "Script not found", "This script doesn't exists.", "Referencing", DiagnosticSeverity.Warning, true)

		);

		public ScriptReferenceAnalyzer(ITenant tenant, Guid microService, Guid component, Guid script) : base(tenant, microService, component, script)
		{
		}

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LoadDirectiveTrivia);
		}

		private void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			if (!IsScriptFile(context))
				return;

			var directive = context.Node as Microsoft.CodeAnalysis.CSharp.Syntax.LoadDirectiveTriviaSyntax;

			var file = directive.File.ValueText;

			if (string.IsNullOrWhiteSpace(file))
				return;

			var tokens = file.Split('/');

			if (tokens.Length < 2)
			{
				context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(GetDescriptor("TP4001"), context.Node.GetLocation()));
				return;
			}

			if (Tenant.GetService<IMicroServiceService>().Select(tokens[0]) is not IMicroService ms)
			{
				context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(GetDescriptor("TP4002"), context.Node.GetLocation(), tokens[0]));
				return;
			}

			try
			{
				MicroService.ValidateMicroServiceReference(ms.Name);
			}
			catch
			{
				context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(GetDescriptor("TP4003"), context.Node.GetLocation(), tokens[0]));
				return;
			}

			if (Tenant.GetService<IComponentService>().SelectComponentByNameSpace(ms.Token, ComponentCategories.NameSpacePublicScript, tokens[1]) is not IComponent component)
			{
				context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(GetDescriptor("TP4004"), context.Node.GetLocation(), tokens[1], tokens[0]));
				return;
			}

			if (tokens.Length == 3)
			{
				var config = Tenant.GetService<IComponentService>().SelectConfiguration(component.Token);
				var scripts = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config);

				if (!scripts.Any(f => string.Compare(Path.GetFileNameWithoutExtension(f.FileName), tokens[2], true) == 0))
					context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(GetDescriptor("TP4005"), context.Node.GetLocation()));
			}
		}
	}
}