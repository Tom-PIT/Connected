using TomPIT.Collections;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
	public interface ISiteMapRoute : ISiteMapElement
	{
		[CIP(CIP.SiteMapViewProvider)]
		string Template { get; }

		string RouteKey { get; }

		ConnectedList<ISiteMapRoute, ISiteMapRoute> Routes { get; }
		bool BeginGroup { get; }

		object Parameters { get; }
		string QueryString { get; }
	}
}
