using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Cdn
{
	public abstract class SubscriptionEventHandler : ProcessHandler, ISubscriptionEventHandler
	{
		public ISubscriptionEvent Event {get;set;}
		public List<IRecipient> Recipients {get;set;}

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected virtual void OnInvoke()
		{

		}
	}
}
