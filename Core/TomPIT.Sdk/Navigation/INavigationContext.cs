using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
	public interface INavigationContext
	{
		string Key { get; }
		bool Enabled { get; }

		[CIP(CIP.RouteKeyProvider)]
		[AA(AA.RouteKeyAnalyzer)]
		string BreadcrumbKey { get; }
		[CIP(CIP.RouteKeyProvider)]
		[AA(AA.RouteKeyAnalyzer)]
		string MenuKey { get; }
	}
}
