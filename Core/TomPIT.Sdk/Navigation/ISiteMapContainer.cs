using TomPIT.Collections;

namespace TomPIT.Navigation
{
	public interface ISiteMapContainer : ISiteMapElement
	{
		string Key { get; }
		ConnectedList<ISiteMapRoute, ISiteMapContainer> Routes { get; }
	}
}
