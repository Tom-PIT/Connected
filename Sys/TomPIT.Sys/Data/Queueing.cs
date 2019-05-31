using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Data
{
	internal class Queueing
	{
		internal const string Queue = "queueworker";
		public void Enqueue(string message, TimeSpan expire, TimeSpan nextVisible)
		{
			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Enqueue(Queue, message, expire, nextVisible, QueueScope.Content);
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.DequeueContent(count, TimeSpan.FromMinutes(2));
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Ping(popReceipt, nextVisible);
		}

		public void Complete(Guid popReceipt)
		{
			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(popReceipt);
		}
	}
}
