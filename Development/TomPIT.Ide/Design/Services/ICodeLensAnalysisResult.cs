namespace TomPIT.Design
{
	public interface ICodeLensAnalysisResult : ICodeAnalysisResult
	{
		ICodeLensCommand Command { get; set; }

	}
}
