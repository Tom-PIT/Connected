using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface ISubscriptionDependencyInjectionMiddleware : IMiddlewareObject
	{
		List<IRecipient> Invoke(List<IRecipient> recipients);
		void Created();
	}
}
