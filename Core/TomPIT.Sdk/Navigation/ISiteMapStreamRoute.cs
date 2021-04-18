namespace TomPIT.Navigation
{
	public interface ISiteMapStreamRoute: ISiteMapElement, ISiteMapAuthorizationElement, ISiteMapRoute
	{
		string Api { get; }
	}
}
