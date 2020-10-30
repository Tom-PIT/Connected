using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Data.Sql;
using TomPIT.Serialization;
using TomPIT.SysDb.Environment;
using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
{
	internal class ReliableMessagingHandler : IReliableMessagingHandler
	{
		public void InsertSubscriber(ITopic topic, string connection, Guid instance)
		{
			var w = new Writer("tompit.message_subscriber_ins");

			w.CreateParameter("@topic", topic.Id);
			w.CreateParameter("@connection", connection);
			w.CreateParameter("@created", DateTime.UtcNow);
			w.CreateParameter("@alive", DateTime.UtcNow);
			w.CreateParameter("@instance", instance);

			w.Execute();
		}

		public void DeleteSubscriber(ITopic topic, string connection)
		{
			var w = new Writer("tompit.message_subscriber_del");

			w.CreateParameter("@topic", topic.Id);
			w.CreateParameter("@connection", connection);

			w.Execute();
		}

		public void InsertMessage(ITopic topic, Guid token, string content, DateTime expire, TimeSpan retryInterval, Guid senderInstance)
		{
			var w = new Writer("tompit.message_ins");

			w.CreateParameter("@topic", topic.Id);
			w.CreateParameter("@message", content, true);
			w.CreateParameter("@created", DateTime.UtcNow);
			w.CreateParameter("@expire", expire);
			w.CreateParameter("@retry_interval", retryInterval.TotalSeconds);
			w.CreateParameter("@token", token);
			w.CreateParameter("@sender_instance", senderInstance);

			w.Execute();
		}

		public void RemoveRecipient(ITopic topic, Guid message, string connection)
		{
			var w = new Writer("tompit.message_recipient_del");

			w.CreateParameter("@topic", topic.Id);
			w.CreateParameter("@message", message);
			w.CreateParameter("@connection", connection);

			w.Execute();
		}

		public List<IRecipient> QueryRecipients()
		{
			return new Reader<Recipient>("tompit.message_recipient_que").Execute().ToList<IRecipient>();
		}

		public void UpdateSubscriber(ISubscriber subscriber, DateTime heartbeat)
		{
			var w = new Writer("tompit.message_subscriber_upd");

			w.CreateParameter("@id", subscriber.Id);
			w.CreateParameter("@alive", heartbeat);

			w.Execute();
		}

		public List<ITopic> QueryTopics()
		{
			return new Reader<Topic>("tompit.message_topic_que").Execute().ToList<ITopic>();
		}

		public ITopic SelectTopic(string name)
		{
			var r = new Reader<Topic>("tompit.message_topic_sel");

			r.CreateParameter("@name", name);

			return r.ExecuteSingleRow();
		}

		public void InsertTopic(IServerResourceGroup resourceGroup, string name)
		{
			var w = new Writer("tompit.message_topic_ins");

			w.CreateParameter("@name", name);
			w.CreateParameter("@resource_group", resourceGroup.GetId());

			w.Execute();
		}

		public void DeleteTopic(ITopic topic)
		{
			var w = new Writer("tompit.message_topic_del");

			w.CreateParameter("@id", topic.Id);

			w.Execute();
		}

		public List<ISubscriber> QuerySubscribers()
		{
			return new Reader<Subscriber>("tompit.message_subscriber_que").Execute().ToList<ISubscriber>();
		}

		public ISubscriber SelectSubscriber(ITopic topic, string connection)
		{
			var r = new Reader<Subscriber>("tompit.message_subscriber_sel");

			r.CreateParameter("@topic", topic.Id);
			r.CreateParameter("@connection", connection);

			return r.ExecuteSingleRow();
		}

		public List<IMessage> QueryMessages()
		{
			return new Reader<Message>("tompit.message_que").Execute().ToList<IMessage>();
		}

		public IMessage SelectMessage(Guid message)
		{
			var r = new Reader<Message>("tompit.message_sel");

			r.CreateParameter("@message", message);

			return r.ExecuteSingleRow();
		}

		public void DeleteMessage(IMessage message)
		{
			var w = new Writer("tompit.message_del");

			w.CreateParameter("@message", message.Id);

			w.Execute();
		}

		public List<IRecipient> QueryRecipients(IMessage message)
		{
			var r = new Reader<Recipient>("tompit.message_recipient_que");

			r.CreateParameter("@message", message.Id);

			return r.Execute().ToList<IRecipient>();
		}

		public IRecipient SelectRecipient(IMessage message, ISubscriber subscriber)
		{
			var r = new Reader<Recipient>("tompit.message_recipient_sel");

			r.CreateParameter("@message", message.Id);
			r.CreateParameter("@subscriber", subscriber.Id);

			return r.ExecuteSingleRow();
		}

		public void DeleteRecipient(IMessage message)
		{
			var w = new Writer("tompit.message_recipient_del");

			w.CreateParameter("@message", message.Id);

			w.Execute();
		}

		public void DeleteRecipient(IMessage message, ISubscriber subscriber)
		{
			var w = new Writer("tompit.message_recipient_del");

			w.CreateParameter("@message", message.Id);
			w.CreateParameter("@subscriber", subscriber.Id);

			w.Execute();
		}

		public void DeleteRecipient(ITopic topic, ISubscriber subscriber)
		{
			var w = new Writer("tompit.message_recipient_clr");

			w.CreateParameter("@topic", topic.Id);
			w.CreateParameter("@subscriber", subscriber.Id);

			w.Execute();
		}

		public void UpdateRecipients(List<IRecipient> recipients)
		{
			var w = new Writer("tompit.message_recipient_upd");

			var dt = new DataTable();

			dt.Columns.Add("id", typeof(long));
			dt.Columns.Add("retry_count", typeof(int));
			dt.Columns.Add("next_visible", typeof(DateTime));

			foreach (var i in recipients)
				dt.Rows.Add(i.Id, i.RetryCount, i.NextVisible);

			w.CreateParameter("@items", dt);

			w.Execute();
		}

		public void Clean(List<IMessage> messages, List<IRecipient> recipients)
		{
			var w = new Writer("tompit.message_clean");

			var m = new JArray();

			foreach (var message in messages)
			{
				m.Add(new JObject
				{
					{ "id", new JValue( message.GetId()) }
				});
			}
			var r = new JArray();

			foreach (var recipient in recipients)
			{
				r.Add(new JObject
				{
					{ "id", new JValue(recipient.GetId()) }
				});
			}
			w.CreateParameter("@messages", Serializer.Serialize(m));
			w.CreateParameter("@recipients", Serializer.Serialize(r));

			w.Execute();
		}
	}
}
