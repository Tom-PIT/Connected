using TomPIT.Collections;

namespace TomPIT.Navigation
{
	public class SiteMapViewContainer : SiteMapViewElement, ISiteMapRouteContainer
	{
		private string _template = null;

		private ConnectedList<ISiteMapRoute, ISiteMapContainer> _items = null;
		public string Key { get; set; }
		public string Tags { get; set; }
		public string SpeculativeRouteKey { get; set; }
		public ConnectedList<ISiteMapRoute, ISiteMapContainer> Routes
		{
			get
			{
				if (_items == null)
					_items = new ConnectedList<ISiteMapRoute, ISiteMapContainer> { Parent = this };

				return _items;
			}
		}

		string ISiteMapRouteContainer.Template
		{
			get
			{
				if (_template == null)
					_template = NavigationExtensions.ResolveRouteTemplate(Context, View);

				return _template;
			}
		}

		protected override void OnDisposing()
		{
			foreach (var route in Routes)
				route.Dispose();

			base.OnDisposing();
		}
	}
}
