using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Notifications
{
	public abstract class EventHandler : ProcessHandler, IEventHandler
	{
		protected EventHandler(IDataModelContext context, string eventName) : base(context)
		{
			EventName = eventName;
		}

		public string EventName { get; }
		public bool Cancel { get; protected set; }

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
