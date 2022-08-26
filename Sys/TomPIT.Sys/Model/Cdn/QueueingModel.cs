using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TomPIT.Caching;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Model.Cdn
{
	internal class QueueingModel : SynchronizedRepository<QueueMessage, long>
	{
		public const string Queue = "queueworker";
		private long _identity = 0;
		private bool _removeDirty = false;

		public QueueingModel(IMemoryCache container) : base(container, "queueMessage")
		{
		}

		protected override void OnInitializing()
		{
			foreach (var j in Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Query())
			{
				if (j.Id > _identity)
					_identity = j.Id;

				Set(j.Id, new QueueMessage(j), TimeSpan.Zero);
			}

			Debug.WriteLine($"Initialized {Count} messages.", "Queue");
		}

		public void Enqueue(string queue, string message, string bufferKey, TimeSpan expire, TimeSpan nextVisible, QueueScope scope)
		{
			Initialize();

			if (!string.IsNullOrWhiteSpace(bufferKey))
			{
				var existing = Where(f => string.Compare(f.Queue, queue, true) == 0 && string.Compare(f.BufferKey, bufferKey, true) == 0);

				if (existing.Any())
				{
					foreach (var ex in existing)
					{
						if (ex.NextVisible <= DateTime.UtcNow)
							return;
					}
				}
			}

			var descriptor = new QueueMessage
			{
				Id = Interlocked.Increment(ref _identity),
				Message = message,
				Created = DateTime.UtcNow,
				Expire = DateTime.UtcNow.Add(expire),
				NextVisible = DateTime.UtcNow.Add(nextVisible),
				Queue = queue,
				Scope = scope,
				BufferKey = bufferKey,
			};

			Set(descriptor.Id, descriptor, TimeSpan.Zero);
		}

		public ImmutableList<IQueueMessage> Dequeue(int count, TimeSpan nextVisible, QueueScope scope, string queue)
		{
#if DEBUG
			var sw = new Stopwatch();

			sw.Start();
#endif

			var results = new List<IQueueMessage>();

			var targets = SelectTargets(count, scope, queue);

			if (targets.IsEmpty)
			{
#if DEBUG
				sw.Stop();

				if (sw.ElapsedMilliseconds > 1)
					Debug.WriteLine($"Dequeue {queue} {sw.ElapsedMilliseconds:n0} ms. (empty)", "Queue");
#endif
				return ImmutableList<IQueueMessage>.Empty;
			}

			foreach (var target in targets)
			{
				try
				{
					target.BeginEdit();

					target.NextVisible = DateTime.UtcNow.Add(nextVisible);
					target.DequeueTimestamp = DateTime.UtcNow;
					target.DequeueCount++;
					target.PopReceipt = Guid.NewGuid();

					results.Add(target);

					target.EndEdit();
				}
				catch
				{
					continue;
				}
			}

#if DEBUG
			sw.Stop();

			Debug.WriteLine($"Dequeue {queue} {sw.ElapsedMilliseconds:n0} ms.", "Queue");
#endif

			return results.ToImmutableList();
		}

		private ImmutableList<QueueMessage> SelectTargets(int count, QueueScope scope, string queue)
		{
			Initialize();

			if (Count == 0)
				return ImmutableList<QueueMessage>.Empty;

			var targets = new List<QueueMessage>();

			foreach (var i in All())
			{
				if (i.Scope != scope || i.NextVisible > DateTime.UtcNow || i.Expire <= DateTime.UtcNow)
					continue;

				if (string.Compare(i.Queue ?? string.Empty, queue ?? string.Empty, true) == 0)
					targets.Add(i);
			}

			if (!targets.Any())
				return ImmutableList<QueueMessage>.Empty;

			var ordered = targets.OrderBy(f => f.NextVisible).ThenBy(f => f.Id);

			if (ordered.Count() <= count)
				return ordered.ToImmutableList();

			return ordered.Take(count).ToImmutableList();
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			if (Select(popReceipt) is not QueueMessage message)
				return;

			try
			{
				message.BeginEdit();
				message.NextVisible = DateTime.UtcNow.Add(nextVisible);
				message.EndEdit();
			}
			catch { }
		}

		public void Complete(Guid popReceipt)
		{
			if (Select(popReceipt) is not QueueMessage message)
				return;

			_removeDirty = true;
			Remove(message.Id);
		}

		public IQueueMessage Select(Guid popReceipt)
		{
			return Get(f => f.PopReceipt == popReceipt);
		}

		public void Flush()
		{
			Initialize();

			var dirty = _removeDirty;
			var messages = All();
			var items = new List<IQueueMessage>();

			_removeDirty = false;

			foreach (var message in messages)
			{
				if (message.Expire < DateTime.UtcNow)
					Remove(message.Id);

				items.Add(message);
			}

			if (dirty || items.Any())
			{
#if DEBUG
				var sw = new Stopwatch();

				sw.Start();
#endif
				Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Update(items);
#if DEBUG
				sw.Stop();

				Debug.WriteLine($"Queue update {sw.ElapsedMilliseconds:n0} ms. {messages.Count} items (with orphanes).", "Queue");
#endif
			}
		}
	}
}
