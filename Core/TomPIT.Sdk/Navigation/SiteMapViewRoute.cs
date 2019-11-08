using TomPIT.Collections;

namespace TomPIT.Navigation
{
	public class SiteMapViewRoute : SiteMapViewElement, ISiteMapRoute
	{
		private ConnectedList<ISiteMapRoute, ISiteMapRoute> _items = null;

		private string _template = null;

		public bool BeginGroup { get; set; }
		string ISiteMapRoute.Template
		{
			get
			{
				if (_template == null)
					_template = NavigationExtensions.ResolveRouteTemplate(Context, View);

				return _template;
			}
		}

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