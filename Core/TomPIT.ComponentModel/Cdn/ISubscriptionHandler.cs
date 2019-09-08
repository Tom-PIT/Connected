using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Cdn
{
	public interface ISubscriptionHandler : IProcessHandler
	{
		List<IRecipient> Invoke(ISubscription subscription);
		void Created(ISubscription subscription);
	}
}
