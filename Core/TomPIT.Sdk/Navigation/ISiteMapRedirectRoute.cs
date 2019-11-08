using Microsoft.AspNetCore.Routing;

namespace TomPIT.Navigation
{
	public interface ISiteMapRedirectRoute
	{
		string RedirectUrl(RouteValueDictionary routes);
	}
}
