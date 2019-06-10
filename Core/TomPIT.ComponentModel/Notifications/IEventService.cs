using Newtonsoft.Json.Linq;
using System;

namespace TomPIT.ComponentModel.Events
{
	public interface IEventService
	{
		Guid Trigger<T>(Guid microService, string name, IEventCallback callback, T e);
		Guid Trigger(Guid microService, string name, IEventCallback callback);
	}
}
