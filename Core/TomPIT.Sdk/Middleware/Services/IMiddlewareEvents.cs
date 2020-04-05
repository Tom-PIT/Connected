using System;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareEvents
	{
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)]string name, object e);
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)]string name);
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)]string name, object e, IMiddlewareCallback callback);
		Guid TriggerEvent([CIP(CIP.DistributedEventProvider)]string name, IMiddlewareCallback callback);
	}
}
