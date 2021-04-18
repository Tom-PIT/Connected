using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
	public class NavigationContext : INavigationContext
	{
		public string Key {get;set;}

		public bool Enabled { get; set; } = true;

		[CIP(CIP.RouteKeyProvider)]
		[AA(AA.RouteKeyAnalyzer)]
		public string BreadcrumbKey {get;set;}

		[CIP(CIP.RouteKeyProvider)]
		[AA(AA.RouteKeyAnalyzer)]
		public string MenuKey {get;set;}
	}
}
