using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	internal class MessageRecipientsModel : SynchronizedRepository<IRecipient, string>
	{
		public MessageRecipientsModel(IMemoryCache container) : base(container, "messagerecipient")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.QueryRecipients();

			foreach (var j in ds)
				Set(GenerateKey(j.Message, j.Connection), j, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			Select(new Guid(tokens[0]), tokens[1]);

			var m = DataModel.Messages.Select(new Guid(tokens[0]));

			if (m == null)
			{
				Remove(id);
				return;
			}

			var s = DataModel.MessageSubscribers.Select(m.Topic, tokens[1]);

			if (s == null)
			{
				Remove(id);
				return;
			}

			var r = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.SelectRecipient(m, s);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IRecipient Select(Guid message, string connection)
		{
			return Get(GenerateKey(message, connection),
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					var m = DataModel.Messages.Select(message);

					if (m == null)
						return null;

					var s = DataModel.MessageSubscribers.Select(m.Topic, connection);

					if (s == null)
						return null;

					return Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.SelectRecipient(m, s);
				});
		}

		public void Load(Guid message)
		{
			var m = DataModel.Messages.Select(message);
			var ds = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.QueryRecipients(m);

			foreach (var i in ds)
				Set(GenerateKey(message, i.Connection), i, TimeSpan.Zero);
		}

		public void Remove(IRecipient recipient)
		{
			Remove(GenerateKey(recipient.Message, recipient.Connection));
		}
		public void Delete(Guid message)
		{
			var ds = Where(f => f.Message == message);

			foreach (var i in ds)
				Remove(GenerateKey(i.Message, i.Connection));

			var m = DataModel.Messages.Select(message);

			if (m == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.DeleteRecipient(m);
		}

		public void Delete(string topic, string connection)
		{
			var t = DataModel.MessageTopics.Ensure(topic);
			var s = DataModel.MessageSubscribers.Select(topic, connection);

			if (s == null)
				return;

			var ds = Where(f => string.Compare(f.Topic, topic, true) == 0
				 && string.Compare(f.Connection, connection, true) == 0);

			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.DeleteRecipient(t, s);

			foreach (var i in ds)
				Remove(GenerateKey(i.Message, i.Connection));
		}

		public ImmutableList<IRecipient> Query(Guid message)
		{
			return Where(f => f.Message == message);
		}

		public ImmutableList<IRecipient> QueryScheduled()
		{
			var ds = Where(f => f.NextVisible < DateTime.UtcNow);
			var batches = new Dictionary<Guid, List<IRecipient>>();

			foreach (var i in ds)
			{
				var m = DataModel.Messages.Select(i.Message);

				if (m == null || m.Expire < DateTime.UtcNow)
					continue;

				i.RetryCount++;
				i.NextVisible = DateTime.UtcNow.Add(m.RetryInterval);

				var t = DataModel.MessageTopics.Select(i.Topic);

				if (t == null)
					continue;

				if (batches.TryGetValue(t.ResourceGroup, out List<IRecipient> items))
					items.Add(i);
				else
				{
					batches.Add(t.ResourceGroup, new List<IRecipient>
					{
						i
					});
				}
			}

			Parallel.ForEach(batches,
				(i) =>
				{
					Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.UpdateRecipients(i.Value);
				});

			return ds;
		}

		public void Delete(Guid message, string connection)
		{
			var m = DataModel.Messages.Select(message);

			if (m == null)
				return;

			var s = DataModel.MessageSubscribers.Select(m.Topic, connection);

			if (s == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.DeleteRecipient(m, s);

			Remove(GenerateKey(message, connection));
		}
	}
}
