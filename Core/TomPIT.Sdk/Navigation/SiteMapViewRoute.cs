using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;

namespace TomPIT.Navigation
{
	public class SiteMapViewRoute : SiteMapViewElement, ISiteMapRoute
	{
		private ConnectedList<ISiteMapRoute, ISiteMapRoute> _items = null;

		private string _template = null;

		public string RouteKey { get; set; }
		public bool BeginGroup { get; set; }
		string ISiteMapRoute.Template
		{
			get
			{
				if (_template == null)
				{
					if (string.IsNullOrWhiteSpace(View) || Context == null)
						return null;

					var view = ComponentDescriptor.View(Context, View);

					if (view.Configuration == null)
						throw new RuntimeException($"{SR.ErrViewNotFound} ({View})");

					_template = view.Configuration.Url;
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