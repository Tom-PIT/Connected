using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public abstract class SubscriptionEventMiddleware : MiddlewareComponent, ISubscriptionEventMiddleware
	{
		public ISubscriptionEvent Event { get; set; }
		public List<IRecipient> Recipients { get; set; }

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
