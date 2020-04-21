using System.Collections.Generic;
using TomPIT.Middleware;
using TomPIT.Search;

namespace TomPIT.IoC
{
	public interface ISearchDependencyInjectionMiddleware : IMiddlewareObject
	{
		List<ISearchEntity> Index(List<ISearchEntity> items);
		ISearchEntity Search(ISearchEntity searchResult, string content);

		List<string> Properties { get; }
	}
}
