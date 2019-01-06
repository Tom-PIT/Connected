namespace TomPIT.Design
{
	public interface ICodeAnalysisResult
	{
		string Text { get; }
		string Description { get; }
		string Value { get; }
	}
}
