using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Search
{
	public enum SearchValidationBehavior
	{
		Retry = 1,
		Complete = 2
	}

	public interface ISearchMiddleware<T> : IMiddlewareComponent
	{
		List<T> Query();
		T Deserialize(string searchResult);

		SearchVerb Verb { get; set; }
		SearchValidationBehavior ValidationFailed { get; }
	}
}
