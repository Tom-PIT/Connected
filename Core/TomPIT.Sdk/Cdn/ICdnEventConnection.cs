using System;
using System.Collections.Generic;

namespace TomPIT.Cdn
{
	public interface ICdnEventConnection : IDisposable
	{
		void Subscribe(List<IEventHubSubscription> events);
		void Unsubscribe(List<IEventHubSubscription> events);
	}
}
