using System.Collections.Generic;

namespace TomPIT.Search
{
	public interface ISearchMiddleware<T> : ISearchComponent
	{
		List<T> Query();
		T Deserialize(string searchResult);
	}
}
