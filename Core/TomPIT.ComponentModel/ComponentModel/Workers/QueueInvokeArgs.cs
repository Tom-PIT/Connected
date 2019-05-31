using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Workers
{
	public class QueueInvokeArgs : EventArguments
	{
		public QueueInvokeArgs(IExecutionContext sender, JObject arguments) : base(sender)
		{
			Arguments = arguments;
		}

		public JObject Arguments { get; }
	}
}
