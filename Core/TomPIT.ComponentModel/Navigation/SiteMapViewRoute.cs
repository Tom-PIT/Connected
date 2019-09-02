using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Navigation
{
	public class SiteMapViewRoute : SiteMapViewElement, ISiteMapRoute
	{
		private ConnectedList<ISiteMapRoute, ISiteMapRoute> _items = null;

		private string _template = null;

		public string RouteKey { get; set; }

		string ISiteMapRoute.Template
		{
			get
			{
				if (_template == null)
				{
					if (string.IsNullOrWhiteSpace(View) || Context == null)
						return null;

					_template = ComponentDescriptor.View(Context, View).Configuration.Url;
				}

				return _template;
			}
		}

		public ConnectedList<ISiteMapRoute, ISiteMapRoute> Items
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