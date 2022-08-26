using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Data.Sql;
using TomPIT.Storage;
using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
{
	internal class QueueHandler : IQueueHandler
	{
		public List<IQueueMessage> Query()
		{
			using var r = new Reader<QueueMessage>("tompit.queue_que");

			return r.Execute().ToList<IQueueMessage>();
		}

		public void Update(List<IQueueMessage> messages)
		{
			using var w = new Writer("tompit.queue_upd");
			var items = new JArray();

			foreach (var item in messages)
			{
				var jo = new JObject
				{
					{"id", item.Id },
					{"next_visible", item.NextVisible },
					{"dequeue_count", item.DequeueCount },
					{"created", item.Created },
					{"expire", item.Expire },
					{"queue", item.Queue },
					{"scope", (int)item.Scope },
					{"message", item.Message }
				};

				if (item.DequeueTimestamp != DateTime.MinValue)
					jo.Add("dequeue_timestamp", item.DequeueTimestamp);

				if (item.PopReceipt != Guid.Empty)
					jo.Add("pop_receipt", item.PopReceipt);

				if (!string.IsNullOrEmpty(item.BufferKey))
					jo.Add("buffer_key", item.BufferKey);

				items.Add(jo);
			};

			w.CreateParameter("@items", items);

			w.Execute();
		}
	}
}