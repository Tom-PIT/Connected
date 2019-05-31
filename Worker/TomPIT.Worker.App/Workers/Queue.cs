using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Compilation;
using TomPIT.ComponentModel.Workers;

namespace TomPIT.Worker.Workers
{
	public class Queue
	{
		public Queue(QueueInvokeArgs e, IQueueWorker worker)
		{
			Args = e;
			Worker = worker;
		}

		private QueueInvokeArgs Args { get; }
		private IQueueWorker Worker { get; }

		public void Invoke()
		{
			Instance.GetService<ICompilerService>().Execute(Worker.MicroService(Instance.Connection), Worker.Invoke, this, Args);
		}
	}
}
