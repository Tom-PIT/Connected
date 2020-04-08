using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Search
{
	public enum SearchValidationBehavior
	{
		Retry = 1,
		Complete = 2
	}

	public interface ISearchComponent : IMiddlewareComponent
	{
		SearchVerb Verb { get; set; }
		SearchValidationBehavior ValidationFailed { get; }

		List<string> Properties { get; }
	}
}
