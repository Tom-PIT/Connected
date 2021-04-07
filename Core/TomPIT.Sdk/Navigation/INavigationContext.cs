using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
	public interface INavigationContext
	{
		string Key { get; }
		bool Enabled { get; }

		[CIP(CIP.RouteKeyProvider)]
		string BreadcrumbKey { get; }
		[CIP(CIP.RouteKeyProvider)]
		string MenuKey { get; }
	}
}
