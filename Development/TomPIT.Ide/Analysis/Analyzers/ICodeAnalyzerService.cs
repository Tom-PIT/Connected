namespace TomPIT.Ide.Analysis.Analyzers
{
	public interface ICodeAnalyzerService
	{
		ICodeAnalyzer GetAnalyzer(string language);
	}
}
