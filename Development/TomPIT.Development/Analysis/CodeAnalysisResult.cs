using TomPIT.Ide.Analysis;

namespace TomPIT.Development.Analysis
{
	internal class CodeAnalysisResult : ICodeAnalysisResult
	{
		public string Text { get; set; }
		public string Description { get; set; }
		public string Value { get; set; }
	}
}
