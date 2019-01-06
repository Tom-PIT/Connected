using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Api.Net;
using TomPIT.Api.Storage;
using TomPIT.Caching;

namespace TomPIT.Sys.Data
{
	internal class MessageRecipients : SynchronizedRepository<IRecipient, string>
	{
		public MessageRecipients(IMemoryCache container) : base(container, "messagerecipient")
		{
		}

		protected override void OnInitializing()
		{
			var rgs = DataModel.ResourceGroups.Query();

			Parallel.ForEach(rgs,
				(i) =>
				{
					var ds = Shell.GetService<IStorageProviderService>().Resolve(i.Token).Messaging.QueryRecipients(i);

					foreach (var j in ds)
						Set(GenerateKey(j.Message, j.Connection), j, TimeSpan.Zero);
				});
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			Select(tokens[0].AsGuid(), tokens[1]);

			var m = DataModel.Messages.Select(tokens[0].AsGuid());

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

			var t = DataModel.MessageTopics.Select(m.Topic);

			if (t == null)
			{
				Remove(id);
				return;
			}

			var rg = DataModel.ResourceGroups.Select(t.ResourceGroup);

			if (rg == null)
			{
				Remove(id);
				return;
			}

			var r = Shell.GetService<IStorageProviderService>().Resolve(Guid.Empty).Messaging.SelectRecipient(rg, m, s);

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

					var t = DataModel.MessageTopics.Select(m.Topic);

					if (t == null)
						return null;

					var rg = DataModel.ResourceGroups.Select(t.ResourceGroup);

					if (rg == null)
						return null;

					return Shell.GetService<IStorageProviderService>().Resolve(Guid.Empty).Messaging.SelectRecipient(rg, m, s);
				});
		}

		public void Load(Guid message)
		{
			var m = DataModel.Messages.Select(message);
			var t = DataModel.MessageTopics.Select(m.Topic);

			if (t == null)
				return;

			var rg = DataModel.ResourceGroups.Select(t.ResourceGroup);

			if (rg == null)
				return;

			var ds = Shell.GetService<IStorageProviderService>().Resolve(Guid.Empty).Messaging.QueryRecipients(rg, m);

			foreach (var i in ds)
				Set(GenerateKey(message, i.Connection), i, TimeSpan.Zero);
		}

		public void Delete(Guid message)
		{
			var ds = Where(f => f.Message == message);

			foreach (var i in ds)
				Remove(GenerateKey(i.Message, i.Connection));

			var m = DataModel.Messages.Select(message);

			if (m == null)
				return;

			var t = DataModel.MessageTopics.Select(m.Topic);

			if (t == null)
				return;

			var rg = DataModel.ResourceGroups.Select(t.ResourceGroup);

			if (rg == null)
				return;

			Shell.GetService<IStorageProviderService>().Resolve(Guid.Empty).Messaging.DeleteRecipient(rg, m);
		}

		public void Delete(string topic, string connection)
		{
			var t = DataModel.MessageTopics.Ensure(topic);
			var s = DataModel.MessageSubscribers.Select(topic, connection);

			if (s == null)
				return;

			var rg = DataModel.ResourceGroups.Select(t.ResourceGroup);

			if (rg == null)
				return;

			var ds = Where(f => string.Compare(f.Topic, topic, true) == 0
				 && string.Compare(f.Connection, connection, true) == 0);

			Shell.GetService<IStorageProviderService>().Resolve(Guid.Empty).Messaging.DeleteRecipient(rg, t, s);

			foreach (var i in ds)
				Remove(GenerateKey(i.Message, i.Connection));
		}

		public List<IRecipient> Query(Guid message)
		{
			return Where(f => f.Message == message);
		}

		public List<IRecipient> QueryScheduled()
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
					var rg = DataModel.ResourceGroups.Select(i.Key);

					if (rg == null)
						return;

					Shell.GetService<IStorageProviderService>().Resolve(Guid.Empty).Messaging.UpdateRecipients(rg, i.Value);
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

			Shell.GetService<IStorageProviderService>().Resolve(Guid.Empty).Messaging.DeleteRecipient(m.ResolveResourceGroup(), m, s);

			Remove(GenerateKey(message, connection));
		}
	}
}
