using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Api.Net;
using TomPIT.Api.Storage;
using TomPIT.Caching;

namespace TomPIT.Sys.Data
{
	internal class MessageSubscribers : SynchronizedRepository<ISubscriber, string>
	{
		public MessageSubscribers(IMemoryCache container) : base(container, "messagesubscriber")
		{
		}

		protected override void OnInitializing()
		{
			var rgs = DataModel.ResourceGroups.Query();

			Parallel.ForEach(rgs,
				(i) =>
				{
					var ds = Shell.GetService<IStorageProviderService>().Resolve(i.Token).Messaging.QuerySubscribers(i);

					foreach (var j in ds)
						Set(GenerateKey(j.Topic, j.Connection), j, TimeSpan.Zero);
				});
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			var t = DataModel.MessageTopics.Ensure(tokens[0]);
			var rg = t.ResolveResourceGroup();
			var r = Shell.GetService<IStorageProviderService>().Resolve(rg.Token).Messaging.SelectSubscriber(rg, t, tokens[1]);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public ISubscriber Select(string topic, Guid instance)
		{
			return Get(f => string.Compare(f.Topic, topic, true) == 0 && f.Instance == instance);
		}

		public ISubscriber Select(string topic, string connection)
		{
			return Get(GenerateKey(topic, connection),
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					var t = DataModel.MessageTopics.Ensure(topic);
					var rg = t.ResolveResourceGroup();

					return Shell.GetService<IStorageProviderService>().Resolve(rg.Token).Messaging.SelectSubscriber(rg, t, connection);
				});
		}

		public List<ISubscriber> Query()
		{
			return All();
		}

		public List<ISubscriber> Query(string topic)
		{
			return Where(f => string.Compare(topic, f.Topic, true) == 0);
		}

		public void Insert(string topic, string connection, Guid instance)
		{
			var t = DataModel.MessageTopics.Ensure(topic);

			if (t == null)
				throw new SysException(string.Format("{0} ({1})", SR.ErrTopicNotFound, topic));

			var rg = t.ResolveResourceGroup();

			Shell.GetService<IStorageProviderService>().Resolve(rg.Token).Messaging.InsertSubscriber(rg, t, connection, instance);

			Refresh(GenerateKey(topic, connection));
		}

		public void Delete(string topic, string connection)
		{
			var s = Select(topic, connection);

			if (s == null)
				return;

			var t = DataModel.MessageTopics.Select(topic);

			if (t == null)
				return;

			var rg = t.ResolveResourceGroup();

			DataModel.MessageRecipients.Delete(topic, connection);
			Shell.GetService<IStorageProviderService>().Resolve(rg.Token).Messaging.DeleteSubscriber(rg, t, connection);

			Remove(GenerateKey(topic, connection));
		}

		public void Heartbeat(string topic, string connection)
		{
			var s = Select(topic, connection);

			if (s == null)
				return;

			var rg = s.ResolveResourceGroup();

			Shell.GetService<IStorageProviderService>().Resolve(rg.Token).Messaging.UpdateSubscriber(rg, s, DateTime.UtcNow);

			Refresh(GenerateKey(topic, connection));
		}
	}
}
