using TomPIT.Collections;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Navigation
{
	public interface ISiteMapRoute : ISiteMapElement
	{
		[CAP(CAP.NavigationUrlProvider)]
		string Template { get; }

		string RouteKey { get; }

		ConnectedList<ISiteMapRoute, ISiteMapRoute> Routes { get; }
	}
}
