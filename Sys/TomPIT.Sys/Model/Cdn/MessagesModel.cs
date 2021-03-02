using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	internal class MessagesModel : SynchronizedRepository<IMessage, Guid>
	{
		public MessagesModel(IMemoryCache container) : base(container, "message")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.QueryMessages();

			foreach (var j in ds)
				Set(j.Token, j, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.SelectMessage(id);

			if (r != null)
			{
				Set(id, r, TimeSpan.Zero);
				return;
			}

			Remove(id);
		}

		public IMessage Select(Guid id)
		{
			return Get(id,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					return Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.SelectMessage(id);
				});
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
			if (DataModel.MessageSubscribers.CandidatesExists(topic))
				return;

			var t = DataModel.MessageTopics.Ensure(topic);

			if (t == null)
				throw new SysException(string.Format("{0} ({1})", SR.ErrTopicNotFound, topic));

			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.InsertMessage(t, token, content, expire, retryInterval, senderInstance);

			Refresh(token);
			DataModel.MessageRecipients.Load(token);
		}

		public void Clean(List<IMessage> messages, List<IRecipient> recipients)
		{
			foreach (var message in messages)
				Remove(message.Token);

			foreach (var recipient in recipients)
				DataModel.MessageRecipients.Remove(recipient);

			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.Clean(messages, recipients);
		}
		public void Delete(Guid message)
		{
			var m = Select(message);

			if (m == null)
				return;

			DataModel.MessageRecipients.Delete(message);
			Shell.GetService<IDatabaseService>().Proxy.Messaging.ReliableMessaging.DeleteMessage(m);

			Remove(message);
		}
	}
}
