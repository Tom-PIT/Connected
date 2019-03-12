using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.Cdn;
using TomPIT.ComponentModel;

namespace TomPIT.Services.Context
{
	internal class ContextCdnService : ContextClient, IContextCdnService
	{
		public ContextCdnService(IExecutionContext context) : base(context)
		{
		}

		public Guid SendMail(string from, string to, string subject, string body)
		{
			return SendMail(from, to, subject, body, null, 0);
		}

		public Guid SendMail(string from, string to, string subject, string body, JArray headers, int attachmentCount)
		{
			return Context.Connection().GetService<IMailService>().Enqueue(from, to, subject, body, headers, attachmentCount, MailFormat.Html, DateTime.UtcNow, DateTime.UtcNow.AddHours(24));
		}

		public void CreateSubscription(string subscription, string primaryKey)
		{
			CreateSubscription(subscription, primaryKey, null);
		}

		public void CreateSubscription(string subscription, string primaryKey, string topic)
		{
			if (!(Context.Connection().GetService<IComponentService>().SelectConfiguration(Context.MicroService.Token, "Subscription", subscription) is TomPIT.ComponentModel.Cdn.ISubscription sub))
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrSubscriptionNotFound, subscription)).WithMetrics(Context);

			Context.Connection().GetService<ISubscriptionService>().CreateSubscription(sub, primaryKey, topic);
		}

		public void TriggerEvent(string subscription, string eventName, string primaryKey, JObject arguments)
		{
			TriggerEvent(subscription, primaryKey, null, eventName, arguments);
		}

		public void TriggerEvent(string subscription, string eventName, string primaryKey, string topic, JObject arguments)
		{
			if (!(Context.Connection().GetService<IComponentService>().SelectConfiguration(Context.MicroService.Token, "Subscription", subscription) is TomPIT.ComponentModel.Cdn.ISubscription sub))
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrSubscriptionNotFound, subscription));

			var ev = sub.Events.FirstOrDefault(f => string.Compare(f.Name, eventName, true) == 0);

			if (ev == null)
				throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrSubscriptionEventNotFound, subscription, eventName)).WithMetrics(Context);

			Context.Connection().GetService<ISubscriptionService>().TriggerEvent(sub, eventName, primaryKey, topic, arguments);
		}
	}
}
