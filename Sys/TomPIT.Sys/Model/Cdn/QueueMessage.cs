using System;
using Newtonsoft.Json;
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

		/*
		 * The cached key and the inputs it was derived from live in one immutable object, so a
		 * reader can never see a key without its inputs. The volatile reference publishes it safely
		 * across threads; a race at worst parses twice with the same result.
		 */
		private sealed class WorkerKeyCache
		{
			public WorkerKeyCache(string message, string queue, string key)
			{
				Message = message;
				Queue = queue;
				Key = key;
			}

			public string Message { get; }
			public string Queue { get; }
			public string Key { get; }
		}

		private volatile WorkerKeyCache _workerKeyCache;

		/*
		 * "{component}/{worker}" from the JSON message, derived and cached, not persisted. Recomputed
		 * if Message or Queue is changed. Never null: anything that doesn't identify a worker is
		 * string.Empty, so all such messages share one group.
		 */
		[JsonIgnore]
		public string WorkerKey
		{
			get
			{
				var message = Message;
				var queue = Queue;
				var cache = _workerKeyCache;

				if (cache is null || !ReferenceEquals(cache.Message, message) || !ReferenceEquals(cache.Queue, queue))
				{
					/*
					 * Only the queueworker payload carries component/worker. Other queues (BigData blocks,
					 * partitions, events...) use different payloads, so they are not parsed at all.
					 */
					var key = string.Equals(queue, QueueingModel.Queue, StringComparison.OrdinalIgnoreCase)
						? ParseWorkerKey(message)
						: string.Empty;

					_workerKeyCache = cache = new WorkerKeyCache(message, queue, key);
				}

				return cache.Key;
			}
		}

		private static string ParseWorkerKey(string message)
		{
			if (string.IsNullOrWhiteSpace(message) || message.TrimStart()[0] != '{')
				return string.Empty;

			try
			{
				using var doc = System.Text.Json.JsonDocument.Parse(message);
				var root = doc.RootElement;

				if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
					return string.Empty;

				var component = root.TryGetProperty("component", out var c) && c.ValueKind == System.Text.Json.JsonValueKind.String ? c.GetString() : null;
				var worker = root.TryGetProperty("worker", out var w) && w.ValueKind == System.Text.Json.JsonValueKind.String ? w.GetString() : null;

				if (string.IsNullOrEmpty(component) && string.IsNullOrEmpty(worker))
					return string.Empty;

				return $"{component}/{worker}";
			}
			catch
			{
				return string.Empty;
			}
		}
	}
}
