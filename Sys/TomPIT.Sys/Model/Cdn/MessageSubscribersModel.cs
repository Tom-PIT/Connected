using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	internal class MessageSubscribersModel : SynchronizedRepository<ISubscriber, string>
	{
		public MessageSubscribersModel(IMemoryCache container) : base(container, "messagesubscriber")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.QuerySubscribers();

			foreach (var j in ds)
				Set(GenerateKey(j.Topic, j.Connection), j, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			var t = DataModel.MessageTopics.Ensure(tokens[0]);
			var r = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.SelectSubscriber(t, tokens[1]);

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

					return Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.SelectSubscriber(t, connection);
				});
		}

		public bool CandidatesExists(string topic)
		{
			return Where(f => string.Compare(f.Topic, topic, true) == 0).Count > 0;
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

			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.InsertSubscriber(t, connection, instance);

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
			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.DeleteSubscriber(t, connection);

			Remove(GenerateKey(topic, connection));
		}

		public void Heartbeat(string topic, string connection)
		{
			var s = Select(topic, connection);

			if (s == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.UpdateSubscriber(s, DateTime.UtcNow);

			Refresh(GenerateKey(topic, connection));
		}
	}
}
