using System;
using System.ComponentModel;
using TomPIT.Storage;

namespace TomPIT.Sys.Model.Cdn
{
	public class QueueMessage : IQueueMessage, IEditableObject
	{
		private bool _isLocked = false;
		private readonly object _sync = new object();
		public QueueMessage()
		{

		}

		public QueueMessage(IQueueMessage message)
		{
			Id = message.Id;
			Message = message.Message;
			Created = message.Created;
			Expire = message.Expire;
			NextVisible = message.NextVisible;
			PopReceipt = message.PopReceipt;
			DequeueCount = message.DequeueCount;
			Queue = message.Queue;
			Scope = message.Scope;
			DequeueTimestamp = message.DequeueTimestamp;
			BufferKey = message.BufferKey;
		}
		public long Id { get; set; }

		public string Message { get; set; }

		public DateTime Created { get; set; }

		public DateTime Expire { get; set; }

		public DateTime NextVisible { get; set; }

		public Guid PopReceipt { get; set; }

		public int DequeueCount { get; set; }

		public string Queue { get; set; }

		public QueueScope Scope { get; set; }

		public DateTime DequeueTimestamp { get; set; }

		public string BufferKey { get; set; }

		public bool IsLocked { get { return _isLocked; } private set { _isLocked = true; } }

		public void BeginEdit()
		{
			if (IsLocked)
				throw new Exception("Locked.");

			lock (_sync)
			{
				if(IsLocked)
					throw new Exception("Locked.");

				IsLocked = true;
			}
		}

		public void CancelEdit()
		{
			_isLocked = false;
		}

		public void EndEdit()
		{
			_isLocked = false;
		}
	}
}
