using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.Cdn;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Workers;
using TomPIT.Environment;

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

		public string CreateMailMessage(string template, string user)
		{
			return CreateMailMessage(template, user, null);
		}

		public string CreateMailMessage(string template, string user, JObject arguments)
		{
			var component = Context.Connection().GetService<IComponentService>().SelectComponent(Context.MicroService.Token, "MailTemplate", template);

			if (component == null || string.Compare(component.Category, "MailTemplate", true) != 0)
				throw new RuntimeException($"{SR.ErrMailTemplateNotFound} ({template})").WithMetrics(Context);

			var url = Context.Connection().GetService<IRuntimeService>().Type == InstanceType.Application
				? Context.RootUrl()
				: Context.Connection().GetService<IInstanceEndpointService>().Url(InstanceType.Application, InstanceVerbs.Post);

			if (string.IsNullOrWhiteSpace(url))
				throw new RuntimeException(SR.ErrNoAppServer).WithMetrics(Context);

			url = $"{url}/sys/mail-template/{component.Token}";

			var e = new JObject();

			if (!string.IsNullOrWhiteSpace(user))
				e.Add("user", user);

			if (arguments != null)
				e.Add("arguments", arguments);

			return Context.Connection().Post<string>(url, e, new Connectivity.HttpRequestArgs
			{
				ReadRawResponse = true
			});
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

		public void TriggerEvent(string eventName, string primaryKey)
		{
			TriggerEvent(eventName, primaryKey, null, null);
		}

		public void TriggerEvent(string eventName, string primaryKey, JObject arguments)
		{
			TriggerEvent(eventName, primaryKey, null, arguments);
		}

		public void TriggerEvent(string eventName, string primaryKey, string topic)
		{
			TriggerEvent(eventName, primaryKey, topic, null);
		}

		public void TriggerEvent(string eventName, string primaryKey, string topic, JObject arguments)
		{
			var tokens = eventName.Split('/');

			if (!(Context.Connection().GetService<IComponentService>().SelectConfiguration(Context.MicroService.Token, "Subscription", tokens[0]) is TomPIT.ComponentModel.Cdn.ISubscription sub))
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrSubscriptionNotFound, tokens[0]));

			var ev = sub.Events.FirstOrDefault(f => string.Compare(f.Name, tokens[1], true) == 0);

			if (ev == null)
				throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrSubscriptionEventNotFound, tokens[0], tokens[1])).WithMetrics(Context);

			Context.Connection().GetService<ISubscriptionService>().TriggerEvent(sub, tokens[1], primaryKey, topic, arguments);
		}

		public void Enqueue(string queue, JObject arguments)
		{
			if (!(Context.Connection().GetService<IComponentService>().SelectConfiguration(Context.MicroService.Token, "Queue", queue) is IQueueWorker worker))
				throw new RuntimeException(SR.ErrQueueWorkerNotFound);

			Context.Connection().GetService<IQueueService>().Enqueue(worker, arguments);
		}

		public void Enqueue(string queue, JObject arguments, TimeSpan expire, TimeSpan nextVisible)
		{
			if (!(Context.Connection().GetService<IComponentService>().SelectConfiguration(Context.MicroService.Token, "Queue", queue) is IQueueWorker worker))
				throw new RuntimeException(SR.ErrQueueWorkerNotFound);

			Context.Connection().GetService<IQueueService>().Enqueue(worker, arguments, expire, nextVisible);
		}
	}
}
