using Newtonsoft.Json.Linq;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Events
{
	public class EventInvokeArguments : EventArguments
	{
		public EventInvokeArguments(IExecutionContext sender, string eventName, JObject arguments) : base(sender)
		{
			Arguments = arguments;
			EventName = eventName;
		}

		public JObject Arguments { get; }
		public string EventName { get; }
	}
}
