using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Workers
{
	public class WorkerInvokeArgs : DataModelContext
	{
		public WorkerInvokeArgs(IExecutionContext sender, JObject state) : base(sender)
		{
			State = state;
		}

		public JObject State { get; }
	}
}
