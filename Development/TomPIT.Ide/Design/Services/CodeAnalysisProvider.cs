using System.Collections.Generic;
using System.Text;
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

		public virtual List<ICodeAnalysisResult> ProvideSnippets(IExecutionContext context, CodeAnalysisArgs e)
		{
			return null;
		}

		protected string RemoveTrailingComma(StringBuilder sb)
		{
			var idx = sb.Length - 3;

			if (idx < 0)
				return sb.ToString();

			if (sb[idx] == ',')
				return sb.Remove(sb.Length - 3, 1).ToString();

			return sb.ToString();
		}
	}
}
