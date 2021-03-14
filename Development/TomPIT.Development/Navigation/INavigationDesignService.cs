using System;
using System.Collections.Immutable;

namespace TomPIT.Development.Navigation
{
	public interface INavigationDesignService
	{
		ImmutableList<INavigationRouteDescriptor> QueryRouteKeys(Guid microservice);
		ImmutableList<string> QuerySiteMapKeys(Guid microService);
	}
}
