namespace TomPIT.Design.Services
{
	public interface ISuggestion
	{
		string Label { get; }
		int Kind { get; }
		string Description { get; }
		string InsertText { get; }
	}
}
