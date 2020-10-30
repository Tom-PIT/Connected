using System.Collections.Generic;

namespace TomPIT.Search
{
	public interface ISearchMiddleware<T> : ISearchComponent
	{
		List<T> Index();
		T Search(string searchResult);
		bool Authorize(T item);
	}
}
