using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Development.Navigation
{
	public interface INavigationDesignService
	{
		List<INavigationRouteDescriptor> QueryRouteKeys(Guid microservice);
		List<string> QuerySiteMapKeys(Guid microService);
	}
}
