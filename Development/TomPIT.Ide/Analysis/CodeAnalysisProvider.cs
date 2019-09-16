using System.Collections.Generic;
using System.Text;
using TomPIT.Ide.Analysis.Analyzers;
using TomPIT.Ide.Analysis.Lenses;
using TomPIT.Middleware;

namespace TomPIT.Ide.Analysis
{
	public abstract class CodeAnalysisProvider : CSharpCodeAnalyzerBase, ICodeAnalysisProvider
	{
		public CodeAnalysisProvider(IMiddlewareContext context) : base(context)
		{

		}

		public virtual ICodeLensAnalysisResult CodeLens(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			return null;
		}

		public virtual List<ICodeAnalysisResult> ProvideHover(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			return null;
		}

		public virtual List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			return null;
		}

		public virtual List<ICodeAnalysisResult> ProvideSnippets(IMiddlewareContext context, CodeAnalysisArgs e)
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
