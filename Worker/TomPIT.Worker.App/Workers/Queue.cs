using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Worker.Workers
{
	public class Queue
	{
		public Queue(string args, IQueueConfiguration handler)
		{
			Args = args;
			Handler = handler;
		}

		private string Args { get; }
		private IQueueConfiguration Handler { get; }

		public IQueueMiddleware HandlerInstance { get; private set; }
		public void Invoke()
		{
			var ms = ((IConfiguration)Handler).MicroService();

			var queueType = Instance.Tenant.GetService<ICompilerService>().ResolveType(ms, Handler, Handler.ComponentName());
			var dataCtx = MiddlewareDescriptor.Current.CreateContext(ms);
			Instance.Tenant.GetService<ICompilerService>().CreateInstance<IQueueMiddleware>(dataCtx, queueType, Args);

			HandlerInstance.Invoke();
		}
	}
}
