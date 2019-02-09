using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TomPIT.Api.Net;
using TomPIT.Api.Storage;
using TomPIT.SysDb.Environment;

namespace TomPIT.StorageProvider.Sql
{
	internal class ReliableMessaging : IReliableMessagingProvider
	{
		public void InsertSubscriber(IServerResourceGroup resourceGroup, ITopic topic, string connection, Guid instance)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_subscriber_ins");

			w.CreateParameter("@topic", topic.Id);
			w.CreateParameter("@connection", connection);
			w.CreateParameter("@created", DateTime.UtcNow);
			w.CreateParameter("@alive", DateTime.UtcNow);
			w.CreateParameter("@instance", instance);

			w.Execute();
		}

		public void DeleteSubscriber(IServerResourceGroup resourceGroup, ITopic topic, string connection)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_subscriber_del");

			w.CreateParameter("@topic", topic.Id);
			w.CreateParameter("@connection", connection);

			w.Execute();
		}

		public void InsertMessage(IServerResourceGroup resourceGroup, ITopic topic, Guid token, string content, DateTime expire, TimeSpan retryInterval, Guid senderInstance)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_ins");

			w.CreateParameter("@topic", topic.Id);
			w.CreateParameter("@message", content, true);
			w.CreateParameter("@created", DateTime.UtcNow);
			w.CreateParameter("@expire", expire);
			w.CreateParameter("@retry_interval", retryInterval.TotalSeconds);
			w.CreateParameter("@token", token);
			w.CreateParameter("@sender_instance", senderInstance);

			w.Execute();
		}

		public void RemoveRecipient(IServerResourceGroup resourceGroup, ITopic topic, Guid message, string connection)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_recipient_del");

			w.CreateParameter("@topic", topic.Id);
			w.CreateParameter("@message", message);
			w.CreateParameter("@connection", connection);

			w.Execute();
		}

		public List<IRecipient> QueryRecipients(IServerResourceGroup resourceGroup)
		{
			return new ResourceGroupReader<Recipient>(resourceGroup, "tompit.message_recipient_que").Execute().ToList<IRecipient>();
		}

		public void UpdateSubscriber(IServerResourceGroup resourceGroup, ISubscriber subscriber, DateTime heartbeat)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_subscriber_upd");

			w.CreateParameter("@id", subscriber.Id);
			w.CreateParameter("@alive", heartbeat);

			w.Execute();
		}

		public List<ITopic> QueryTopics(IServerResourceGroup resourceGroup)
		{
			return new ResourceGroupReader<Topic>(resourceGroup, "tompit.message_topic_que").Execute().ToList<ITopic>();
		}

		public ITopic SelectTopic(IServerResourceGroup resourceGroup, string name)
		{
			var r = new ResourceGroupReader<Topic>(resourceGroup, "tompit.message_topic_sel");

			r.CreateParameter("@name", name);

			return r.ExecuteSingleRow();
		}

		public void InsertTopic(IServerResourceGroup resourceGroup, string name)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_topic_ins");

			w.CreateParameter("@name", name);
			w.CreateParameter("@resource_group", resourceGroup.GetId());

			w.Execute();
		}

		public void DeleteTopic(IServerResourceGroup resourceGroup, ITopic topic)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_topic_del");

			w.CreateParameter("@id", topic.Id);

			w.Execute();
		}

		public List<ISubscriber> QuerySubscribers(IServerResourceGroup resourceGroup)
		{
			return new ResourceGroupReader<Subscriber>(resourceGroup, "tompit.message_subscriber_que").Execute().ToList<ISubscriber>();
		}

		public ISubscriber SelectSubscriber(IServerResourceGroup resourceGroup, ITopic topic, string connection)
		{
			var r = new ResourceGroupReader<Subscriber>(resourceGroup, "tompit.message_subscriber_sel");

			r.CreateParameter("@topic", topic.Id);
			r.CreateParameter("@connection", connection);

			return r.ExecuteSingleRow();
		}

		public List<IMessage> QueryMessages(IServerResourceGroup resourceGroup)
		{
			return new ResourceGroupReader<Message>(resourceGroup, "tompit.message_que").Execute().ToList<IMessage>();
		}

		public IMessage SelectMessage(IServerResourceGroup resourceGroup, Guid message)
		{
			var r = new ResourceGroupReader<Message>(resourceGroup, "tompit.message_sel");

			r.CreateParameter("@message", message);

			return r.ExecuteSingleRow();
		}

		public void DeleteMessage(IServerResourceGroup resourceGroup, IMessage message)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_del");

			w.CreateParameter("@message", message.Id);

			w.Execute();
		}

		public List<IRecipient> QueryRecipients(IServerResourceGroup resourceGroup, IMessage message)
		{
			var r = new ResourceGroupReader<Recipient>(resourceGroup, "tompit.message_recipient_que");

			r.CreateParameter("@message", message.Id);

			return r.Execute().ToList<IRecipient>();
		}

		public IRecipient SelectRecipient(IServerResourceGroup resourceGroup, IMessage message, ISubscriber subscriber)
		{
			var r = new ResourceGroupReader<Recipient>(resourceGroup, "tompit.message_recipient_sel");

			r.CreateParameter("@message", message.Id);
			r.CreateParameter("@subscriber", subscriber.Id);

			return r.ExecuteSingleRow();
		}

		public void DeleteRecipient(IServerResourceGroup resourceGroup, IMessage message)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_recipient_del");

			w.CreateParameter("@message", message.Id);

			w.Execute();
		}

		public void DeleteRecipient(IServerResourceGroup resourceGroup, IMessage message, ISubscriber subscriber)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_recipient_del");

			w.CreateParameter("@message", message.Id);
			w.CreateParameter("@subscriber", subscriber.Id);

			w.Execute();
		}

		public void DeleteRecipient(IServerResourceGroup resourceGroup, ITopic topic, ISubscriber subscriber)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_recipient_clr");

			w.CreateParameter("@topic", topic.Id);
			w.CreateParameter("@subscriber", subscriber.Id);

			w.Execute();
		}

		public void UpdateRecipients(IServerResourceGroup resourceGroup, List<IRecipient> recipients)
		{
			var w = new ResourceGroupWriter(resourceGroup, "tompit.message_recipient_upd");

			var dt = new DataTable();

			dt.Columns.Add("id", typeof(long));
			dt.Columns.Add("retry_count", typeof(int));
			dt.Columns.Add("next_visible", typeof(DateTime));

			foreach (var i in recipients)
				dt.Rows.Add(i.Id, i.RetryCount, i.NextVisible);

			w.CreateParameter("@items", dt);

			w.Execute();
		}
	}
}
