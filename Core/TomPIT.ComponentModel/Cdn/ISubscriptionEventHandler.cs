using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Cdn
{
	public interface ISubscriptionEventHandler : IProcessHandler
	{
		ISubscriptionEvent Event { get; set; }
		List<IRecipient> Recipients { get; set; }

		void Invoke();
	}
}
