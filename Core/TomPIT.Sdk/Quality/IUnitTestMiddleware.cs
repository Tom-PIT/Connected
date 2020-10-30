using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Quality
{
	public interface IUnitTestMiddleware : IMiddlewareComponent
	{
		void Initialize();
		void Invoke(List<IUnitTestMessage> messages);
	}
}
