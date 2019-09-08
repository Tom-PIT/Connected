using Newtonsoft.Json.Linq;
using System;

namespace TomPIT.ComponentModel.Events
{
	public interface IEventService
	{
		Guid Trigger<T>(IDistributedEvent ev, IEventCallback callback, T e);
		Guid Trigger(IDistributedEvent ev, IEventCallback callback);
	}
}
