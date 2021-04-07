using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
	public class NavigationContext : INavigationContext
	{
		public string Key {get;set;}

		public bool Enabled { get; set; } = true;

		[CIP(CIP.RouteKeyProvider)] 
		public string BreadcrumbKey {get;set;}

		[CIP(CIP.RouteKeyProvider)] 
		public string MenuKey {get;set;}
	}
}
