using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	internal class QueueingModel : SynchronizedRepository<IQueueMessage, string>
	{
		internal const string Queue = "queueworker";
		private object DequeueSync = new object();

		public QueueingModel(IMemoryCache container) : base(container, "queueMessage")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Query();

			foreach (var j in ds)
				Set(j.Id, j, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(id);

			if (r != null)
			{
				Set(id, r, TimeSpan.Zero);
				return;
			}

			Remove(id);
		}

		public void Enqueue(string queue, string message, TimeSpan expire, TimeSpan nextVisible, QueueScope scope)
		{
			Refresh(Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Insert(queue, message, expire, nextVisible, scope));
		}

		public List<IQueueMessage> Dequeue(int count, TimeSpan nextVisible, QueueScope scope, string queue)
		{
			List<IQueueMessage> result = null;

			lock (DequeueSync)
			{
				result = SelectTargets(count, scope, queue);

				if (result == null || result.Count == 0)
					return new List<IQueueMessage>();

				foreach (var target in result)
				{
					if (target is IQueueMessageModifier modifier)
						modifier.Modify(DateTime.UtcNow.Add(nextVisible), DateTime.UtcNow, target.DequeueCount + 1, Guid.NewGuid());
				}
			}

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Update(result);

			return result;
		}

		private List<IQueueMessage> SelectTargets(int count, QueueScope scope, string queue)
		{
			Initialize();

			if (Count == 0)
				return null;

			var targets = string.IsNullOrWhiteSpace(queue)
				? Where(f => f.Scope == scope && f.NextVisible <= DateTime.UtcNow && f.Expire > DateTime.UtcNow)
				: Where(f => f.Scope == scope && f.NextVisible <= DateTime.UtcNow && f.Expire > DateTime.UtcNow && string.Compare(f.Queue, queue, true) == 0);

			if (targets.Count == 0)
				return null;

			var ordered = targets.OrderBy(f => f.NextVisible).ThenBy(f => f.Id);

			if (ordered.Count() <= count)
				return ordered.ToList();

			return ordered.Take(count).ToList();
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			var message = Select(popReceipt);

			if (message == null)
				return;

			if (message is IQueueMessageModifier modifier)
				modifier.Modify(DateTime.UtcNow.Add(nextVisible), message.DequeueTimestamp, message.DequeueCount, message.PopReceipt);

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Update(new List<IQueueMessage> { message });
		}

		public void Complete(Guid popReceipt)
		{
			var message = Select(popReceipt);

			if (message == null)
				return;

			lock (DequeueSync)
			{
				Remove(message.Id);
			}

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(message);
		}

		public IQueueMessage Select(Guid popReceipt)
		{
			return Get(f => f.PopReceipt == popReceipt);
		}

		public void Recycle()
		{
			Initialize();

			var orphanes = Where(f => f.Expire < DateTime.UtcNow);

			foreach (var message in orphanes)
			{
				lock (DequeueSync)
				{
					Remove(message.Id);
				}

				Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(message);
			}
		}
	}
}
