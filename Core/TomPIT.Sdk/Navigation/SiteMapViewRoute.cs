using TomPIT.Collections;
using TomPIT.Middleware;

namespace TomPIT.Navigation
{
	public class SiteMapViewRoute : SiteMapViewElement, ISiteMapRoute
	{
		private ConnectedList<ISiteMapRoute, ISiteMapRoute> _items = null;

		private string _template = null;

		public bool BeginGroup { get; set; }
		public object Parameters { get; set; }
		public string QueryString { get; set; }
		string ISiteMapRoute.Template
		{
			get
			{
				if (_template == null)
				{
					using var context = new MiddlewareContext(MiddlewareDescriptor.Current.Tenant.Url);
					
					_template = NavigationExtensions.ResolveRouteTemplate(context, View);
				}
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