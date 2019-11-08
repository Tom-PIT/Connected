namespace TomPIT.Ide.Analysis.Lenses
{
	public interface ICodeLensAnalysisResult : ICodeAnalysisResult
	{
		ICodeLensCommand Command { get; set; }

	}
}
