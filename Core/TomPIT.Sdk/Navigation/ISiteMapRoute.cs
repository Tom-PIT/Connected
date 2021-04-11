using TomPIT.Collections;
using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
	public interface ISiteMapRoute : ISiteMapElement
	{
		string Template { get; }

		string RouteKey { get; }

		ConnectedList<ISiteMapRoute, ISiteMapRoute> Routes { get; }
		bool BeginGroup { get; }

		object Parameters { get; }
		string QueryString { get; }

		[CIP(CIP.NavigationContextProvider)]
		[AA(AA.NavigationContextAnalyzer)]
		string NavigationContext { get; }
	}
}
