using System;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Connectivity;
using TomPIT.Serialization;

namespace TomPIT.Cdn
{
	internal class QueueService : TenantObject, IQueueService
	{
		public QueueService(ITenant tenant) : base(tenant)
		{
		}

		public void Enqueue<T>(IQueueWorker worker, string bufferKey, T arguments)
		{
			Enqueue(worker, bufferKey, arguments, TimeSpan.FromDays(2), TimeSpan.Zero);
		}

		public void Enqueue<T>(IQueueWorker worker, string bufferKey, T arguments, TimeSpan expire, TimeSpan nextVisible)
		{
			Instance.SysProxy.Queue.Enqueue(worker.Configuration().Component, worker.Name, bufferKey, arguments is null ? null : Serializer.Serialize(arguments), expire, nextVisible);
		}
	}
}
