using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Sys.Caching;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	public class MessagesModel : IdentityRepository<IMessage, Guid>
	{
		public MessagesModel(IMemoryCache container) : base(container, "message")
		{
		}

		public IMessage Select(Guid id)
		{
			return Get(id);
		}

		public ImmutableList<IMessage> Query()
		{
			return All();
		}

		public ImmutableList<IMessage> Query(string topic)
		{
			return Where(f => string.Compare(f.Topic, topic, true) == 0);
		}

		public void Insert(string topic, Guid token, string content, DateTime expire, TimeSpan retryInterval, Guid senderInstance)
		{
			Set(token, new Message
			{
				Created = DateTime.UtcNow,
				Expire = expire,
				Id = Increment(),
				RetryInterval = retryInterval,
				Text = content,
				Token = token,
				Topic = topic
			}, TimeSpan.Zero);

			DataModel.MessageRecipients.Load(token);
		}

		public void Clean(List<IMessage> messages, List<IRecipient> recipients)
		{
			foreach (var message in messages)
				Remove(message.Token);

			foreach (var recipient in recipients)
				DataModel.MessageRecipients.Remove(recipient);
		}

		public void Delete(Guid message)
		{
			if (Select(message) is not Message m)
				return;

			DataModel.MessageRecipients.Delete(message);

			Remove(message);
		}
	}
}
