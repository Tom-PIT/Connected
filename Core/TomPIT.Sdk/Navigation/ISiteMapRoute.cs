using TomPIT.Collections;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
	public interface ISiteMapRoute : ISiteMapElement
	{
		[CIP(CIP.SiteMapViewUrlProvider)]
		string Template { get; }

		string RouteKey { get; }

		ConnectedList<ISiteMapRoute, ISiteMapRoute> Routes { get; }
	}
}
