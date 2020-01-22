using System;
using System.Linq;
using TomPIT.Cdn;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareQueue : MiddlewareObject, IMiddlewareQueue
	{
		public void Enqueue([CIP(CIP.QueueWorkersProvider)]string queue, object arguments)
		{
			Context.Tenant.GetService<IQueueService>().Enqueue(ResolveQueue(queue), arguments);
		}

		public void Enqueue([CIP(CIP.QueueWorkersProvider)]string queue, object arguments, TimeSpan expire, TimeSpan nextVisible)
		{
			Context.Tenant.GetService<IQueueService>().Enqueue(ResolveQueue(queue), arguments, expire, nextVisible);
		}

		private IQueueWorker ResolveQueue(string qualifier)
		{
			var config = ComponentDescriptor.Queue(Context, qualifier);

			config.Validate();

			return config.Configuration.Workers.FirstOrDefault(f => string.Compare(f.Name, config.Element, true) == 0);
		}
	}
}
