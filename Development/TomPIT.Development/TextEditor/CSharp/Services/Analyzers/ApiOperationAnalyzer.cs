using System.Linq;
using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;

namespace TomPIT.Development.TextEditor.CSharp.Services.Analyzers
{
	internal class ApiOperationAnalyzer : ComponentAnalyzer
	{
		protected override void OnAnalyzeIdentifier(string identifier)
		{
			var tokens = identifier.Split('/');

			if (tokens.Length != 3)
			{
				Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3002"), Context.Node.GetLocation()));
				return;
			}

			if (AnalyzeMicroService(tokens[0]) is not IMicroService ms)
				return;

			if (AnalyzeComponent(ms, ComponentCategories.Api, tokens[1]) is not IComponent component)
				return;

			if (Analyzer.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is not IApiConfiguration configuration)
				return;

			if (configuration.Operations.FirstOrDefault(f => string.Compare(f.Name, tokens[2], true) == 0) is null)
				Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3008"), Context.Node.GetLocation(), "Operation", tokens[2], tokens[1]));
		}
	}
}
