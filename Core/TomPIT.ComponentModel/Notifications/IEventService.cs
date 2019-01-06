using Newtonsoft.Json.Linq;
using System;

namespace TomPIT.ComponentModel.Events
{
	public interface IEventService
	{
		Guid Trigger(Guid microService, string name, JObject e, IEventCallback callback);
	}
}
