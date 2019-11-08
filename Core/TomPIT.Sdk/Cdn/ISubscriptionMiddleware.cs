using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public interface ISubscriptionMiddleware : IMiddlewareComponent
	{
		List<IRecipient> Invoke(ISubscription subscription);
		void Created(ISubscription subscription);
	}
}
