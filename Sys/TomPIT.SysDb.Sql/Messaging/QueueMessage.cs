using System;
using TomPIT.Data.Sql;
using TomPIT.Storage;

namespace TomPIT.SysDb.Sql.Messaging
{
	internal class QueueMessage : DatabaseRecord, IQueueMessage
	{
		public long Id { get; set; }
		public string Message { get; set; }
		public DateTime Created { get; set; }
		public DateTime Expire { get; set; }
		public DateTime NextVisible { get; set; }
		public Guid PopReceipt { get; set; }
		public int DequeueCount { get; set; }
		public string Queue { get; set; }
		public DateTime DequeueTimestamp { get; set; }
		public QueueScope Scope { get; set; }
		public string BufferKey { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Id = GetLong("id");
			Message = GetString("message");
			Created = GetDate("created");
			Expire = GetDate("expire");
			NextVisible = GetDate("next_visible");
			PopReceipt = GetGuid("pop_receipt");
			DequeueCount = GetInt("dequeue_count");
			Queue = GetString("queue");
			DequeueTimestamp = GetDate("dequeue_timestamp");
			Scope = GetValue("scope", QueueScope.System);
			BufferKey = GetString("buffer_key");
		}
	}
}
