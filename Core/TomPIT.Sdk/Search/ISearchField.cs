namespace TomPIT.Search
{
	public interface ISearchField
	{
		string Name { get; }
		string Value { get; }

		SearchTermVector TermVector { get; }
		SearchMode Mode { get; }
		float Boost { get; }
	}
}
