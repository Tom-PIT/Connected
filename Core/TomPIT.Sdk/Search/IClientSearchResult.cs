namespace TomPIT.Search
{
	public interface IClientSearchResult : ISearchResult
	{
		object Entity { get; }
	}
}
