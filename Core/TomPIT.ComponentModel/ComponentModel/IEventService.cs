using System;
using Newtonsoft.Json.Linq;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel
{
	public interface IEventService
	{
		Guid Trigger(Guid microService, string name, JObject e, IEventCallback callback);
	}
}
