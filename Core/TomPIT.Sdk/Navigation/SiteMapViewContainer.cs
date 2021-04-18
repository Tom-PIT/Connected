using TomPIT.Collections;
using TomPIT.Middleware;
using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
	public class SiteMapViewContainer : SiteMapViewElement, ISiteMapRouteContainer, ISitemapNavigationContextElement
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

		public object Parameters { get; set; }

		public string QueryString { get; set; }

		[CIP(CIP.NavigationContextProvider)]
		[AA(AA.NavigationContextAnalyzer)]
		public string NavigationContext {get;set;}

		string ISiteMapRouteContainer.Template
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
	}
}