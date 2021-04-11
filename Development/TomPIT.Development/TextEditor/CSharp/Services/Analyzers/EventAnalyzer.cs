using System.Linq;
using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;

namespace TomPIT.Development.TextEditor.CSharp.Services.Analyzers
{
	internal class EventAnalyzer : ComponentAnalyzer
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

			if (AnalyzeComponent(ms, ComponentCategories.DistributedEvent, tokens[1]) is not IComponent component)
				return;

			if (Analyzer.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is not IDistributedEventsConfiguration configuration)
				return;

			if (configuration.Events.FirstOrDefault(f => string.Compare(f.Name, tokens[2], true) == 0) is null)
				Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3008"), Context.Node.GetLocation(), "Event", tokens[2], tokens[1]));
		}
	}
}
