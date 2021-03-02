using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Data.Sql;
using TomPIT.Serialization;
using TomPIT.Storage;
using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
{
	internal class QueueHandler : IQueueHandler
	{
		public void Delete(IQueueMessage message)
		{
			using var w = new Writer("tompit.queue_del");

			w.CreateParameter("@id", message.Id);

			w.Execute();
		}

		public IQueueMessage Select(string id)
		{
			using var r = new Reader<QueueMessage>("tompit.queue_sel");

			r.CreateParameter("@id", Convert.ToInt64(id));

			return r.ExecuteSingleRow();
		}

		public List<IQueueMessage> Query()
		{
			using var r = new Reader<QueueMessage>("tompit.queue_que");

			return r.Execute().ToList<IQueueMessage>();
		}

		public string Insert(string queue, string message, string bufferKey, TimeSpan expire, TimeSpan nextVisible, QueueScope scope)
		{
			using var w = new LongWriter("tompit.queue_ins");

			w.CreateParameter("@message", message);
			w.CreateParameter("@queue", queue);
			w.CreateParameter("@expire", DateTime.UtcNow.Add(expire));
			w.CreateParameter("@next_visible", DateTime.UtcNow.Add(nextVisible));
			w.CreateParameter("@scope", scope);
			w.CreateParameter("@created", DateTime.UtcNow);
			w.CreateParameter("@bufferKey", bufferKey, true);

			w.Execute();

			return w.Result.ToString();
		}

		public string Insert(string queue, IQueueContent content, string bufferKey, TimeSpan expire, TimeSpan nextVisible, QueueScope scope)
		{
			return Insert(queue, content.Serialize(), bufferKey, expire, nextVisible, scope);
		}

		public void Update(List<IQueueMessage> messages)
		{
			var items = new JArray();

			foreach (var item in messages)
			{
				items.Add(new JObject
				{
					{"id", item.Id },
					{"next_visible", item.NextVisible },
					{"dequeue_count", item.DequeueCount },
					{"dequeue_timestamp", item.DequeueTimestamp },
					{"pop_receipt", item.PopReceipt }
				});
			};

			using var w = new Writer("tompit.queue_upd");

			w.CreateParameter("@items", Serializer.Serialize(items));

			w.Execute();
		}
	}
}