﻿using System;
using TomPIT.Storage;

namespace TomPIT.Search.Services
{
	internal class QueueMessage : IQueueMessage
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
	}
}