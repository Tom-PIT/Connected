using System;
using TomPIT.Data.Sql;
using TomPIT.Storage;
using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
{
	internal class QueueMessage : DatabaseRecord, IQueueMessage, IQueueMessageModifier
	{
		public string Id { get; set; }
		public string Message { get; set; }
		public DateTime Created { get; set; }
		public DateTime Expire { get; set; }
		public DateTime NextVisible { get; set; }
		public Guid PopReceipt { get; set; }
		public int DequeueCount { get; set; }
		public string Queue { get; set; }
		public DateTime DequeueTimestamp { get; set; }
		public QueueScope Scope { get; set; }
		public void Modify(DateTime nextVisible, DateTime dequeueTimestamp, int dequeueCount, Guid popReceipt)
		{
			NextVisible = nextVisible;
			DequeueTimestamp = dequeueTimestamp;
			DequeueCount = dequeueCount;
			PopReceipt = popReceipt;
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			Id = GetLong("id").ToString();
			Message = GetString("message");
			Created = GetDate("created");
			Expire = GetDate("expire");
			NextVisible = GetDate("next_visible");
			PopReceipt = GetGuid("pop_receipt");
			DequeueCount = GetInt("dequeue_count");
			Queue = GetString("queue");
			DequeueTimestamp = GetDate("dequeue_timestamp");
			Scope = GetValue("scope", QueueScope.System);
		}
	}
}
