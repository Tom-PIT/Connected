using System.Collections.Generic;

namespace TomPIT.IoC
{
	public interface IDependencyInjectionService
	{
		List<IApiDependencyInjectionObject> QueryApiDependencies(string api, object arguments);
		List<ISearchDependencyInjectionMiddleware> QuerySearchDependencies(string catalog, object arguments);
		List<ISubscriptionDependencyInjectionMiddleware> QuerySubscriptionDependencies(string subscription, object arguments);
		List<ISubscriptionEventDependencyInjectionMiddleware> QuerySubscriptionEventDependencies(string subscriptionEvents, object arguments);
	}
}
