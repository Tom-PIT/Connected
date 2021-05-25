namespace TomPIT.Navigation
{
	public interface ISiteMapRouteContainer : ISiteMapContainer, INavigationContextElement
	{
		string Template { get; }
		string RouteKey { get; }
		object Parameters { get; }
		string QueryString { get; }
	}
}
