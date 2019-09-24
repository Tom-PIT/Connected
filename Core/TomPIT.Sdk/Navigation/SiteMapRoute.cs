using TomPIT.Collections;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
	public class SiteMapRoute : SiteMapElement, ISiteMapRoute
	{
		private ConnectedList<ISiteMapRoute, ISiteMapRoute> _items = null;

		[CIP(CIP.SiteMapViewUrlProvider)]
		public string Template { get; set; }

		public string RouteKey { get; set; }

		public ConnectedList<ISiteMapRoute, ISiteMapRoute> Routes
		{
			get
			{
				if (_items == null)
					_items = new ConnectedList<ISiteMapRoute, ISiteMapRoute> { Parent = this };

				return _items;
			}
		}
	}
}
