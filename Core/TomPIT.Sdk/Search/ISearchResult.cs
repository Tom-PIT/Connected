using System;

namespace TomPIT.Search
{
	public interface ISearchResult
	{
		Guid Catalog { get; }
		string Content { get; }
		float Score { get; }
		string Text { get; }
		string Title { get; }
	}
}