using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.IoT
{
	public class IoTDataArguments : EventArguments
	{
		public IoTDataArguments(IExecutionContext sender, JObject data) : base(sender)
		{
			Data = data;
		}

		public JObject Data { get; }
	}
}
