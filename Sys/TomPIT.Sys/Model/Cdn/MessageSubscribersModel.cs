using System;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Sys.Caching;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	public class MessageSubscribersModel : IdentityRepository<ISubscriber, string>
	{
		public MessageSubscribersModel(IMemoryCache container) : base(container, "messagesubscriber")
		{
		}

		public ISubscriber Select(string topic, Guid instance)
		{
			return Get(f => string.Compare(f.Topic, topic, true) == 0 && f.Instance == instance);
		}

		public ISubscriber Select(string topic, string connection)
		{
			return Get(GenerateKey(topic, connection));
		}

		public bool CandidatesExists(string topic)
		{
			return Where(f => string.Compare(f.Topic, topic, true) == 0).Any();
		}
		public ImmutableList<ISubscriber> Query()
		{
			return All();
		}

		public ImmutableList<ISubscriber> Query(string topic)
		{
			return Where(f => string.Compare(topic, f.Topic, true) == 0);
		}

		public void Insert(string topic, string connection, Guid instance)
		{
			Set(GenerateKey(topic, connection), new Subscriber
			{
				Alive = DateTime.Now,
				Connection = connection,
				Instance = instance,
				Created = DateTime.UtcNow,
				Id = Increment(),
				Topic = topic
			}, TimeSpan.Zero);
		}

		public void Delete(string topic, string connection)
		{
			Remove(GenerateKey(topic, connection));
			DataModel.MessageRecipients.Delete(topic, connection);
		}

		public void Heartbeat(string topic, string connection)
		{
			if (Select(topic, connection) is Subscriber subscriber)
				subscriber.Alive = DateTime.UtcNow;
		}
	}
}
