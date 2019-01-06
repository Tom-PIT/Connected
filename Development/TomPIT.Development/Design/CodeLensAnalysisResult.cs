namespace TomPIT.Design
{
	internal class CodeLensAnalysisResult : CodeAnalysisResult, ICodeLensAnalysisResult
	{
		public CodeLensAnalysisResult(string text, string value) : base(text, value, null)
		{

		}

		public ICodeLensCommand Command { get; set; }
	}
}
