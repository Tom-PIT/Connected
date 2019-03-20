using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Storage;
using TomPIT.SysDb.Environment;
using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
{
	internal class QueueHandler : IQueueHandler
	{
		public void Delete(IServerResourceGroup resourceGroup, IQueueMessage message)
		{
			Delete(resourceGroup, message.PopReceipt);
		}

		public void Delete(IServerResourceGroup resourceGroup, Guid popReceipt)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.queue_del");

			w.CreateParameter("@pop_receipt", popReceipt);

			w.Execute();
		}

		public IQueueMessage Select(IServerResourceGroup resourceGroup, Guid popReceipt)
		{
			var r = new ResourceGroupReader<QueueMessage>(resourceGroup, "tompit.queue_sel");

			r.CreateParameter("@pop_receipt", popReceipt);

			return r.ExecuteSingleRow();
		}

		public IQueueMessage DequeueSystem(IServerResourceGroup resourceGroup, string queue)
		{
			return DequeueSystem(resourceGroup, queue, TimeSpan.FromMinutes(5));
		}

		public List<IQueueMessage> DequeueSystem(IServerResourceGroup resourceGroup, string queue, int count)
		{
			return DequeueSystem(resourceGroup, queue, count, TimeSpan.FromMinutes(5));
		}

		public IQueueMessage DequeueSystem(IServerResourceGroup resourceGroup, string queue, TimeSpan nextVisible)
		{
			var r = DequeueSystem(resourceGroup, queue, 1, nextVisible);

			if (r == null || r.Count == 0)
				return null;

			return r[0];
		}

		public List<IQueueMessage> DequeueSystem(IServerResourceGroup resourceGroup, string queue, int count, TimeSpan nextVisible)
		{
			var r = new ResourceGroupReader<QueueMessage>(resourceGroup, "tompit.queue_dequeue");

			r.CreateParameter("@queue", queue);
			r.CreateParameter("@next_visible", DateTime.UtcNow.Add(nextVisible));
			r.CreateParameter("@count", @count);
			r.CreateParameter("@date", DateTime.UtcNow);

			return r.Execute().ToList<IQueueMessage>();
		}

		public IQueueMessage DequeueContent(IServerResourceGroup resourceGroup)
		{
			return DequeueContent(resourceGroup, TimeSpan.FromMinutes(5));
		}

		public List<IQueueMessage> DequeueContent(IServerResourceGroup resourceGroup, int count)
		{
			return DequeueContent(resourceGroup, count, TimeSpan.FromMinutes(5));
		}

		public IQueueMessage DequeueContent(IServerResourceGroup resourceGroup, TimeSpan nextVisible)
		{
			var r = DequeueContent(resourceGroup, 1, nextVisible);

			if (r == null || r.Count == 0)
				return null;

			return r[0];
		}

		public List<IQueueMessage> DequeueContent(IServerResourceGroup resourceGroup, int count, TimeSpan nextVisible)
		{
			var r = new ResourceGroupReader<QueueMessage>(resourceGroup, "tompit.queue_dequeue_content");

			r.CreateParameter("@next_visible", DateTime.UtcNow.Add(nextVisible));
			r.CreateParameter("@count", @count);
			r.CreateParameter("@date", DateTime.UtcNow);

			return r.Execute().ToList<IQueueMessage>();
		}

		public void Enqueue(IServerResourceGroup resourceGroup, string queue, string message, TimeSpan expire, TimeSpan nextVisible, QueueScope scope)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.queue_enqueue");

			w.CreateParameter("@message", message);
			w.CreateParameter("@queue", queue);
			w.CreateParameter("@expire", Convert.ToInt32(expire.TotalSeconds));
			w.CreateParameter("@next_visible", DateTime.UtcNow.Add(nextVisible));
			w.CreateParameter("@scope", scope);
			w.CreateParameter("@created", DateTime.UtcNow);

			w.Execute();
		}

		public void Enqueue(IServerResourceGroup resourceGroup, string queue, IQueueContent content, TimeSpan expire, TimeSpan nextVisible, QueueScope scope)
		{
			Enqueue(resourceGroup, queue, content.Serialize(), expire, nextVisible, scope);
		}

		public void Ping(IServerResourceGroup resourceGroup, Guid popReceipt, TimeSpan nextVisible)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.queue_upd");

			w.CreateParameter("@pop_receipt", popReceipt);
			w.CreateParameter("@next_visible", DateTime.UtcNow.Add(nextVisible));

			w.Execute();
		}
	}
}