using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
	public interface ISitemapNavigationContextElement
	{
		[CIP(CIP.NavigationContextProvider)]
		[AA(AA.NavigationContextAnalyzer)]
		string NavigationContext { get; }
	}
}
