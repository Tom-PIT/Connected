namespace TomPIT.Search
{
	public enum SearchResultMessageType
	{
		Information = 1,
		Warning = 2,
		Error = 3
	}

	public interface ISearchResultMessage
	{
		SearchResultMessageType Type { get; }
		string Text { get; }
	}
}
