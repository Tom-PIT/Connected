using System.Collections.Generic;
using TomPIT.Ide.Analysis.Lenses;
using TomPIT.Middleware;

namespace TomPIT.Ide.Analysis
{
	public interface ICodeAnalysisProvider
	{
		List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e);
		List<ICodeAnalysisResult> ProvideSnippets(IMiddlewareContext context, CodeAnalysisArgs e);
		List<ICodeAnalysisResult> ProvideHover(IMiddlewareContext context, CodeAnalysisArgs e);
		ICodeLensAnalysisResult CodeLens(IMiddlewareContext context, CodeAnalysisArgs e);
	}
}
