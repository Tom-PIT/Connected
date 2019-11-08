using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public interface ISubscriptionEventMiddleware : IMiddlewareComponent
	{
		ISubscriptionEvent Event { get; set; }
		List<IRecipient> Recipients { get; set; }

		void Invoke();
	}
}
