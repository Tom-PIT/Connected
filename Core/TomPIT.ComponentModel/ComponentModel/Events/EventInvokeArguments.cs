using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Events
{
	public class EventInvokeArguments : DataModelContext
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
