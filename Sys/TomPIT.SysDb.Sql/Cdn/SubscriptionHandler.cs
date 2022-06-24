using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Cdn;

namespace TomPIT.SysDb.Sql.Cdn
{
	internal class SubscriptionHandler : ISubscriptionHandler
	{
		public void DeleteEvent(ISubscriptionEvent d)
		{
			using var w = new Writer("tompit.subscription_event_del");

			w.CreateParameter("@id", d.GetId());

			w.Execute();
		}

		public void DeleteSubscriber(ISubscriber subscriber)
		{
			using var w = new Writer("tompit.subscriber_del");

			w.CreateParameter("@id", subscriber.GetId());

			w.Execute();
		}

		public void Delete(ISubscription subscription)
		{
			using var w = new Writer("tompit.subscription_del");

			w.CreateParameter("@id", subscription.GetId());

			w.Execute();
		}

		public void Delete(Guid handler)
		{
			using var w = new Writer("tompit.subscription_clr");

			w.CreateParameter("@handler", handler);

			w.Execute();
		}

		public void InsertEvent(ISubscription subscription, Guid token, string name, DateTime created, string arguments)
		{
			using var w = new Writer("tompit.subscription_event_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@subscription", subscription.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@created", created);
			w.CreateParameter("@arguments", arguments, true);

			w.Execute();
		}

		public void InsertSubscriber(ISubscription subscription, Guid token, SubscriptionResourceType type, string resourcePrimaryKey, List<string> tags)
		{
			using var w = new Writer("tompit.subscriber_ins");

			w.CreateParameter("@subscription", subscription.GetId());
			w.CreateParameter("@token", token);
			w.CreateParameter("@resource_type", type);
			w.CreateParameter("@resource_primary_key", resourcePrimaryKey);

			if (tags is not null && tags.Any())
				w.CreateParameter("@tags", string.Join(',', tags));

			w.Execute();
		}

		public void InsertSubscribers(ISubscription subscription, List<IRecipient> subscribers)
		{
			using var w = new Writer("tompit.subscriber_ins_batch");
			var a = new JArray();

			foreach (var i in subscribers)
			{
				var subscriber = new JObject
				{
					{"resource_type", Convert.ToInt32(i.Type) },
					{"resource_primary_key", i.ResourcePrimaryKey },
					{"token", i.Token }
				};

				if (i.Tags is not null && i.Tags.Any())
					subscriber.Add("tags", string.Join(',', i.Tags));

				a.Add(subscriber);
			};


			w.CreateParameter("@subscription", subscription.GetId());
			w.CreateParameter("@items", a);

			w.Execute();
		}

		public List<ISubscriptionEvent> QueryEvents()
		{
			using var r = new Reader<SubscriptionEvent>("tompit.subscription_event_que");

			return r.Execute().ToList<ISubscriptionEvent>();
		}

		public List<ISubscriber> QuerySubscribers(ISubscription subscription)
		{
			using var r = new Reader<Subscriber>("tompit.subscriber_que");

			r.CreateParameter("@subscription", subscription.GetId());

			return r.Execute().ToList<ISubscriber>();
		}

		public ISubscriptionEvent SelectEvent(Guid token)
		{
			using var r = new Reader<SubscriptionEvent>("tompit.subscription_event_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public ISubscriber SelectSubscriber(Guid token)
		{
			using var r = new Reader<Subscriber>("tompit.subscriber_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public ISubscriber SelectSubscriber(ISubscription subscription, SubscriptionResourceType type, string resourcePrimaryKey)
		{
			using var r = new Reader<Subscriber>("tompit.subscriber_sel");

			r.CreateParameter("@subscription", subscription.GetId());
			r.CreateParameter("@resource_type", type);
			r.CreateParameter("@resource_primary_key", resourcePrimaryKey);

			return r.ExecuteSingleRow();
		}

		public void Insert(Guid token, Guid handler, string topic, string primaryKey)
		{
			using var w = new Writer("tompit.subscription_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@handler", handler);
			w.CreateParameter("@topic", topic, true);
			w.CreateParameter("@primary_key", primaryKey);

			w.Execute();
		}

		public ISubscription Select(Guid token)
		{
			using var r = new Reader<Subscription>("tompit.subscription_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public ISubscription Select(Guid handler, string topic, string primaryKey)
		{
			using var r = new Reader<Subscription>("tompit.subscription_sel");

			r.CreateParameter("@handler", handler);
			r.CreateParameter("@topic", topic, true);
			r.CreateParameter("@primary_key", primaryKey);

			return r.ExecuteSingleRow();
		}
	}
}
