using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.IoT
{
	public class IoTTransactionInvokeArguments : DataModelContext
	{
		public IoTTransactionInvokeArguments(IExecutionContext sender, JObject data) : base(sender)
		{
			Data = data;
		}

		public JObject Data { get; }
	}
}
