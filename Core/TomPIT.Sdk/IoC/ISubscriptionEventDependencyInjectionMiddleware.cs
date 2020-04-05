using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface ISubscriptionEventDependencyInjectionMiddleware : IMiddlewareObject
	{
		List<IRecipient> Invoke(List<IRecipient> recipients);
	}
}
