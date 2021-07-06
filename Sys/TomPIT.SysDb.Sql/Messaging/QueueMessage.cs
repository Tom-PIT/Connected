using System;
using System.Threading;
using TomPIT.Data.Sql;
using TomPIT.Storage;
using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
{
    internal class QueueMessage : DatabaseRecord, IQueueMessage, IQueueMessageModifier
    {
        private int _isDirty;
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
        public string BufferKey { get; set; }
        public bool Modify(DateTime nextVisible, DateTime dequeueTimestamp, int dequeueCount, Guid popReceipt)
        {
            if (_isDirty == 1)
                return false;

            lock (this)
            {
                if (_isDirty == 1)
                    return false;

                Interlocked.Exchange(ref _isDirty, 1);

                NextVisible = nextVisible;
                DequeueTimestamp = dequeueTimestamp;
                DequeueCount = dequeueCount;
                PopReceipt = popReceipt;
                
                return true;
            }
        }

        public void Reset()
        {
            Interlocked.Exchange(ref _isDirty, 0);
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
            BufferKey = GetString("buffer_key");
        }


    }
}
