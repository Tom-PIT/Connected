using System.Collections.Generic;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Design.Services
{
	public abstract class CodeAnalysisProvider : AnalyzerBase, ICodeAnalysisProvider
	{
		public CodeAnalysisProvider(IExecutionContext context) : base(context)
		{

		}

		public virtual ICodeLensAnalysisResult CodeLens(IExecutionContext context, CodeAnalysisArgs e)
		{
			return null;
		}

		public virtual List<ICodeAnalysisResult> ProvideHover(IExecutionContext context, CodeAnalysisArgs e)
		{
			return null;
		}

		public virtual List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			return null;
		}
	}
}
