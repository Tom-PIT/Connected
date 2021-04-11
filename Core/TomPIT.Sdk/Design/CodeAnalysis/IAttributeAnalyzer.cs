using Microsoft.CodeAnalysis.Diagnostics;

namespace TomPIT.Design.CodeAnalysis
{
	public interface IAttributeAnalyzer
	{
		void Analyze(SyntaxNodeAnalysisContext context, IDiagnosticAnalyzer analyzer);
	}
}
