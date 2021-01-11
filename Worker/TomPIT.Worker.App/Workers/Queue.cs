using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Worker.Workers
{
	public class Queue
	{
		public Queue(string args, IQueueWorker worker)
		{
			Args = args;
			Worker = worker;
		}

		private string Args { get; }
		private IQueueWorker Worker { get; }

		public IQueueMiddleware HandlerInstance { get; private set; }
		public void Invoke()
		{
			var ms = Worker.Configuration().MicroService();

			var queueType = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(ms, Worker, Worker.Name);
			using var dataCtx = new MicroServiceContext(ms);
			HandlerInstance = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IQueueMiddleware>(dataCtx, queueType, Args);

			HandlerInstance.Invoke();
		}
	}
}
