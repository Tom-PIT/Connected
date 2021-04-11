using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Reflection;

namespace TomPIT.Development.TextEditor.CSharp.Services.Analyzers
{
	internal abstract class ComponentAnalyzer : AttributeAnalyzer
	{
		protected override void OnAnalyze()
		{
			var text = ResolveExpressionText();

			if (string.IsNullOrWhiteSpace(text))
				return;

			text = text.Trim('"');

			if (string.IsNullOrWhiteSpace(text))
			{
				if (EmptyIdentifierReportsException)
					Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3001"), Context.Node.GetLocation()));

				return;
			}

			OnAnalyzeIdentifier(text);
		}

		protected virtual bool EmptyIdentifierReportsException => true;

		protected virtual void OnAnalyzeIdentifier(string identifier)
		{
		}

		protected IMicroService AnalyzeMicroService(string name)
		{
			var ms = Analyzer.Tenant.GetService<IMicroServiceService>().Select(name);

			if (ms is null)
			{
				Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3003"), Context.Node.GetLocation(), name));
				return null;
			}

			if (!CheckReference(ms))
				return null;

			return ms;
		}

		private string ResolveExpressionText()
		{
			if (Context.Node is AttributeArgumentSyntax argument)
			{
				if (argument.Expression.TryResolveValue(Context.SemanticModel, out object value))
					return value as string;
			}
			else if (Context.Node is ArgumentSyntax arg)
			{
				if (arg.Expression.TryResolveValue(Context.SemanticModel, out object value))
					return value as string;
			}
			else if (Context.Node is AssignmentExpressionSyntax asn)
			{
				if (asn.Right.TryResolveValue(Context.SemanticModel, out object value))
					return value as string;
			}

			return null;
		}

		private bool CheckReference(IMicroService microService)
		{
			if (microService.Token == Analyzer.MicroService.Token)
				return true;

			var references = Analyzer.Tenant.GetService<IDiscoveryService>().MicroServices.References.Select(microService.Token);

			if (references is null)
			{
				Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3004"), Context.Node.GetLocation(), microService.Name));
				return false;
			}

			try
			{
				Analyzer.MicroService.ValidateMicroServiceReference(microService.Name);
			}
			catch
			{
				Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3004"), Context.Node.GetLocation(), microService.Name));
				return false;
			}

			return true;
		}

		protected IComponent AnalyzeComponent(IMicroService microService, string category, string component)
		{
			if (Analyzer.Tenant.GetService<IComponentService>().SelectComponent(microService.Token, category, component) is not IComponent result)
			{
				Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3005"), Context.Node.GetLocation(), component, microService.Name));
				return null;
			}

			return result;
		}
	}
}
