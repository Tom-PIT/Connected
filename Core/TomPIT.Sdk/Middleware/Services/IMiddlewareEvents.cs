using System;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;
using DP = TomPIT.Annotations.Design.DefinitionProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareEvents
	{
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)][DP(DP.DistributedEventProvider)]string name, [CIP(CIP.DistributedEventPropertyProvider)]object e);
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)][DP(DP.DistributedEventProvider)]string name);
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)][DP(DP.DistributedEventProvider)]string name, [CIP(CIP.DistributedEventPropertyProvider)]object e, IMiddlewareCallback callback);
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)][DP(DP.DistributedEventProvider)]string name, IMiddlewareCallback callback);
	}
}
