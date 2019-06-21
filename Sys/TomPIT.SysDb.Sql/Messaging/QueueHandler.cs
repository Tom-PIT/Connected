using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Storage;
using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
{
	internal class QueueHandler : IQueueHandler
	{
		public void Delete(IQueueMessage message)
		{
			Delete(message.PopReceipt);
		}

		public void Delete(Guid popReceipt)
		{
			var w = new Writer("tompit.queue_del");

			w.CreateParameter("@pop_receipt", popReceipt);

			w.Execute();
		}

		public IQueueMessage Select(Guid popReceipt)
		{
			var r = new Reader<QueueMessage>("tompit.queue_sel");

			r.CreateParameter("@pop_receipt", popReceipt);

			return r.ExecuteSingleRow();
		}

		public IQueueMessage DequeueSystem(string queue)
		{
			return DequeueSystem(queue, TimeSpan.FromMinutes(5));
		}

		public List<IQueueMessage> DequeueSystem(string queue, int count)
		{
			return DequeueSystem(queue, count, TimeSpan.FromMinutes(5));
		}

		public IQueueMessage DequeueSystem(string queue, TimeSpan nextVisible)
		{
			var r = DequeueSystem(queue, 1, nextVisible);

			if (r == null || r.Count == 0)
				return null;

			return r[0];
		}

		public List<IQueueMessage> DequeueSystem(string queue, int count, TimeSpan nextVisible)
		{
			var r = new Reader<QueueMessage>("tompit.queue_dequeue");

			r.CreateParameter("@queue", queue);
			r.CreateParameter("@next_visible", DateTime.UtcNow.Add(nextVisible));
			r.CreateParameter("@count", @count);
			r.CreateParameter("@date", DateTime.UtcNow);

			return r.Execute().ToList<IQueueMessage>();
		}

		public IQueueMessage DequeueContent()
		{
			return DequeueContent(TimeSpan.FromMinutes(5));
		}

		public List<IQueueMessage> DequeueContent(int count)
		{
			return DequeueContent(count, TimeSpan.FromMinutes(5));
		}

		public IQueueMessage DequeueContent(TimeSpan nextVisible)
		{
			var r = DequeueContent(1, nextVisible);

			if (r == null || r.Count == 0)
				return null;

			return r[0];
		}

		public List<IQueueMessage> DequeueContent(int count, TimeSpan nextVisible)
		{
			var r = new Reader<QueueMessage>("tompit.queue_dequeue_content");

			r.CreateParameter("@next_visible", DateTime.UtcNow.Add(nextVisible));
			r.CreateParameter("@count", @count);
			r.CreateParameter("@date", DateTime.UtcNow);

			return r.Execute().ToList<IQueueMessage>();
		}

		public void Enqueue(string queue, string message, TimeSpan expire, TimeSpan nextVisible, QueueScope scope)
		{
			var w = new Writer("tompit.queue_enqueue");

			w.CreateParameter("@message", message);
			w.CreateParameter("@queue", queue);
			w.CreateParameter("@expire", DateTime.UtcNow.Add(expire));
			w.CreateParameter("@next_visible", DateTime.UtcNow.Add(nextVisible));
			w.CreateParameter("@scope", scope);
			w.CreateParameter("@created", DateTime.UtcNow);

			w.Execute();
		}

		public void Enqueue(string queue, IQueueContent content, TimeSpan expire, TimeSpan nextVisible, QueueScope scope)
		{
			Enqueue(queue, content.Serialize(), expire, nextVisible, scope);
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			var w = new Writer("tompit.queue_upd");

			w.CreateParameter("@pop_receipt", popReceipt);
			w.CreateParameter("@next_visible", DateTime.UtcNow.Add(nextVisible));

			w.Execute();
		}
	}
}