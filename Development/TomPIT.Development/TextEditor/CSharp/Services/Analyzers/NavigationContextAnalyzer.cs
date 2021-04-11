using Microsoft.CodeAnalysis;
using TomPIT.Navigation;

namespace TomPIT.Development.TextEditor.CSharp.Services.Analyzers
{
	internal class NavigationContextAnalyzer : ComponentAnalyzer
	{
		protected override void OnAnalyzeIdentifier(string identifier)
		{
			if(Analyzer.Tenant.GetService<INavigationService>().SelectNavigationContext(identifier) is null)
				Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3009"), Context.Node.GetLocation(), "NavigationContext"));
		}

		protected override bool EmptyIdentifierReportsException => false;
	}
}
