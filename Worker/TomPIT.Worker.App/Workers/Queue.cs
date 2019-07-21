using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Handlers;
using TomPIT.ComponentModel.Workers;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.Worker.Workers
{
	public class Queue
	{
		public Queue(string args, IQueueHandlerConfiguration handler)
		{
			Args = args;
			Handler = handler;
		}

		private string Args { get; }
		private IQueueHandlerConfiguration Handler { get; }

		public IQueueHandler HandlerInstance { get; private set; }
		public void Invoke()
		{
			var ms = ((IConfiguration)Handler).MicroService(Instance.Connection);

			var queueType = Instance.GetService<ICompilerService>().ResolveType(ms, Handler, Handler.ComponentName(Instance.Connection));
			var ctx = ExecutionContext.Create(Instance.Connection.Url, Instance.GetService<IMicroServiceService>().Select(ms));
			var dataCtx = new DataModelContext(ctx);
			HandlerInstance = queueType.CreateInstance<IQueueHandler>(new object[] {dataCtx });

			if (!string.IsNullOrWhiteSpace(Args))
				Types.Populate(Args, HandlerInstance);

			HandlerInstance.Invoke();
		}
	}
}
