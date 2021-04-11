using TomPIT.Collections;
using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;
namespace TomPIT.Navigation
{
	public class SiteMapRoute : SiteMapElement, ISiteMapRoute, ISitemapNavigationContextElement
	{
		private ConnectedList<ISiteMapRoute, ISiteMapRoute> _items = null;

		public string Template { get; set; }

		public string RouteKey { get; set; }
		public bool BeginGroup { get; set; }
		public object Parameters { get; set; }
		public string QueryString { get; set; }
		public ConnectedList<ISiteMapRoute, ISiteMapRoute> Routes
		{
			get
			{
				if (_items == null)
					_items = new ConnectedList<ISiteMapRoute, ISiteMapRoute> { Parent = this };

				return _items;
			}
		}

		[CIP(CIP.NavigationContextProvider)]
		[AA(AA.NavigationContextAnalyzer)]
		public string NavigationContext {get;set;}
	}
}
