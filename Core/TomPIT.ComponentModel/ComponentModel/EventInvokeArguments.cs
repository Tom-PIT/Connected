using Newtonsoft.Json.Linq;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel
{
	public class EventInvokeArguments : EventArguments
	{
		public EventInvokeArguments(IApplicationContext sender, string eventName, JObject arguments) : base(sender)
		{
			Arguments = arguments;
			EventName = eventName;
		}

		public JObject Arguments { get; }
		public string EventName { get; }
	}
}
