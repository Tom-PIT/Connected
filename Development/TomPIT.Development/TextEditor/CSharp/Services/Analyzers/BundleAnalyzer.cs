using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;

namespace TomPIT.Development.TextEditor.CSharp.Services.Analyzers
{
	internal class BundleAnalyzer : ComponentAnalyzer
	{
		protected override void OnAnalyzeIdentifier(string identifier)
		{
			var tokens = identifier.Split('/');

			if (tokens.Length != 2)
			{
				Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3002"), Context.Node.GetLocation()));
				return;
			}

			if (AnalyzeMicroService(tokens[0]) is not IMicroService ms)
				return;

			if (AnalyzeComponent(ms, ComponentCategories.ScriptBundle, tokens[1]) is null)
				return;
		}
	}
}
