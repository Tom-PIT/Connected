using TomPIT.Annotations;
using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Reflection;

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

		public string QueueName { get; private set; }
		public bool Invoke(ProcessBehavior behavior)
		{
			var ms = Worker.Configuration().MicroService();

			var queueType = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(ms, Worker, Worker.Name);
			using var dataCtx = new MicroServiceContext(ms);
			HandlerInstance = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IQueueMiddleware>(dataCtx, queueType, Args);

			if (behavior == ProcessBehavior.Parallel)
			{
				var att = HandlerInstance.GetType().FindAttribute<ProcessBehaviorAttribute>();

				if (att.Behavior == ProcessBehavior.Queued)
				{
					QueueName = att.QueueName;
					return false;
				}
			}

			HandlerInstance.Invoke();

			return true;
		}
	}
}
