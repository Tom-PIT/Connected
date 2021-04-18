using Microsoft.CodeAnalysis.Diagnostics;
using TomPIT.Design.CodeAnalysis;

namespace TomPIT.Development.TextEditor.CSharp.Services.Analyzers
{
	internal abstract class AttributeAnalyzer : IAttributeAnalyzer
	{
		public void Analyze(SyntaxNodeAnalysisContext context, IDiagnosticAnalyzer analyzer)
		{
			Context = context;
			Analyzer = analyzer;

			OnAnalyze();
		}

		protected SyntaxNodeAnalysisContext Context { get; private set; }
		protected IDiagnosticAnalyzer Analyzer { get; private set; }
		protected virtual void OnAnalyze()
		{

		}
	}
}
