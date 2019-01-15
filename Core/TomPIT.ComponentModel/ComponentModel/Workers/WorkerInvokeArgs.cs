using Newtonsoft.Json.Linq;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Workers
{
	public class WorkerInvokeArgs : EventArguments
	{
		public WorkerInvokeArgs(IExecutionContext sender, JObject state) : base(sender)
		{
			State = state;
		}

		public JObject State { get; }
	}
}
