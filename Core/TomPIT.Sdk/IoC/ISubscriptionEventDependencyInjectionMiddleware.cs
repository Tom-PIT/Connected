using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface ISubscriptionEventDependencyInjectionMiddleware : IMiddlewareObject
	{
		void Invoke(List<IRecipient> recipients);
		List<IRecipient> QueryRecipients(List<IRecipient> recipients);
		void Validate();
		void Commit();
		void Rollback();
	}
}
