using TomPIT.Collections;

namespace TomPIT.Navigation
{
	public class SiteMapContainer : SiteMapElement, ISiteMapContainer
	{
		private ConnectedList<ISiteMapRoute, ISiteMapContainer> _items = null;
		public string Key { get; set; }
		public string Tags { get; set; }
		public ConnectedList<ISiteMapRoute, ISiteMapContainer> Routes
		{
			get
			{
				if (_items == null)
					_items = new ConnectedList<ISiteMapRoute, ISiteMapContainer> { Parent = this };

				return _items;
			}
		}
	}
}
