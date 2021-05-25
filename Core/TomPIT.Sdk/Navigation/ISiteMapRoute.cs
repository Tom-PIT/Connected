using TomPIT.Collections;

namespace TomPIT.Navigation
{
	public interface ISiteMapRoute : ISiteMapElement, INavigationContextElement
	{
		string Template { get; }

		string RouteKey { get; }

		ConnectedList<ISiteMapRoute, ISiteMapRoute> Routes { get; }
		bool BeginGroup { get; }

		object Parameters { get; }
		string QueryString { get; }
	}
}
