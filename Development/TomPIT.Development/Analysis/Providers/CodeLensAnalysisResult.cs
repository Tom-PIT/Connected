using TomPIT.Ide.Analysis.Lenses;

namespace TomPIT.Development.Analysis.Providers
{
	internal class CodeLensAnalysisResult : CodeAnalysisResult, ICodeLensAnalysisResult
	{
		public CodeLensAnalysisResult(string text, string value) : base(text, value, null)
		{

		}

		public ICodeLensCommand Command { get; set; }
	}
}
