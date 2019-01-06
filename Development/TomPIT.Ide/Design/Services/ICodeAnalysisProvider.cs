using System.Collections.Generic;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Design
{
	public interface ICodeAnalysisProvider
	{
		List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e);
		List<ICodeAnalysisResult> ProvideHover(IExecutionContext context, CodeAnalysisArgs e);
		ICodeLensAnalysisResult CodeLens(IExecutionContext context, CodeAnalysisArgs e);
	}
}
