using System;
using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.Cdn
{
	public interface ISubscription : IConfiguration, ISourceCode
	{
		[Obsolete]
		IServerEvent Subscribe { get; }
		[Obsolete]
		IServerEvent Subscribed { get; }

		ListItems<ISubscriptionEvent> Events { get; }
	}
}
