using System;
using TomPIT.Data;
using TomPIT.Storage;

namespace TomPIT.Sys.Model.Cdn
{
	public class QueueMessage : EditableRecord, IQueueMessage
	{
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

		private string _workerKey;
		private bool _workerKeyParsed;

		/* "{component}/{worker}" from the JSON message. Parsed once, not persisted. */
		public string WorkerKey
		{
			get
			{
				if (!_workerKeyParsed)
				{
					_workerKey = ParseWorkerKey(Message);
					_workerKeyParsed = true;
				}

				return _workerKey;
			}
		}

		private static string ParseWorkerKey(string message)
		{
			if (string.IsNullOrWhiteSpace(message))
				return null;

			try
			{
				using var doc = System.Text.Json.JsonDocument.Parse(message);
				var root = doc.RootElement;

				var component = root.TryGetProperty("component", out var c) ? c.GetString() : null;
				var worker = root.TryGetProperty("worker", out var w) ? w.GetString() : null;

				return $"{component}/{worker}";
			}
			catch
			{
				return null;
			}
		}
	}
}
