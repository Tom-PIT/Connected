using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Messaging
{
	public interface IEventMiddleware : IMiddlewareComponent
	{
		List<IOperationResponse> Responses { get; }
		void Invoke(string eventName);
	}
}
