using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Sys.Caching;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	internal class MessageRecipientsModel : IdentityRepository<IRecipient, string>
	{
		public MessageRecipientsModel(IMemoryCache container) : base(container, "messagerecipient")
		{
		}

		public IRecipient Select(Guid message, string connection)
		{
			return Get(GenerateKey(message, connection));
		}

		public void Load(Guid message)
		{
			var m = DataModel.Messages.Select(message);
			var subscribers = DataModel.MessageSubscribers.Query(m.Topic);

			foreach (var subscriber in subscribers)
			{
				Set(GenerateKey(message, subscriber.Connection), new MessageRecipient
				{
					Connection = subscriber.Connection,
					Content = m.Text,
					Id = Increment(),
					Message = message,
					NextVisible = DateTime.UtcNow,
					RetryCount = 10,
					Topic = m.Topic
				});
			}
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
		}

		public void Delete(string topic, string connection)
		{
			var t = DataModel.MessageTopics.Ensure(topic);
			var s = DataModel.MessageSubscribers.Select(topic, connection);

			if (s == null)
				return;

			var ds = Where(f => string.Compare(f.Topic, topic, true) == 0
				 && string.Compare(f.Connection, connection, true) == 0);

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

			foreach (var i in ds)
			{
				var m = DataModel.Messages.Select(i.Message);

				if (m is null || m.Expire < DateTime.UtcNow)
				{
					DataModel.Messages.Delete(m.Token);
					continue;
				}

				if (i.RetryCount > 10)
				{
					Remove(i);
					continue;
				}

				i.RetryCount++;
				i.NextVisible = DateTime.UtcNow.Add(m.RetryInterval);
			}

			return ds;
		}

		public void Delete(Guid message, string connection)
		{
			Remove(GenerateKey(message, connection));
		}
	}
}
