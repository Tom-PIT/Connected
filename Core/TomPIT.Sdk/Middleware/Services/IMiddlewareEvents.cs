using System;
using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;
using DP = TomPIT.Annotations.Design.DefinitionProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareEvents
	{
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)][DP(DP.DistributedEventProvider)][AA(AA.EventAnalyzer)]string name, [CIP(CIP.DistributedEventPropertyProvider)]object e);
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)][DP(DP.DistributedEventProvider)][AA(AA.EventAnalyzer)] string name);
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)][DP(DP.DistributedEventProvider)][AA(AA.EventAnalyzer)] string name, [CIP(CIP.DistributedEventPropertyProvider)]object e, IMiddlewareCallback callback);
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)][DP(DP.DistributedEventProvider)][AA(AA.EventAnalyzer)] string name, IMiddlewareCallback callback);
	}
}
