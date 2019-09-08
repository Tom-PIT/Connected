using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.Cdn;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.Handlers;
using TomPIT.Data;
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
			return CreateMailMessage<object>(template, user, null);
		}

		public string CreateMailMessage<T>(string template, string user, T arguments)
		{
			var tokens = template.Split("/");
			var ms = Context.Connection().GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({tokens[0]})").WithMetrics(Context);

			Context.MicroService.ValidateMicroServiceReference(Context.Connection(), ms.Name);

			var component = Context.Connection().GetService<IComponentService>().SelectComponent(ms.Token, "MailTemplate", tokens[1]);

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
				e.Add("arguments", Types.Serialize(arguments));

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
			var config = ComponentDescriptor.Subscription(new DataModelContext(Context), subscription);

			config.Validate();

			Context.Connection().GetService<ISubscriptionService>().CreateSubscription(config.Configuration, primaryKey, topic);
		}

		public void Enqueue<T>([CodeAnalysisProvider(ExecutionContext.QueueWorkerProvider)]string queue, T arguments)
		{
			Context.Connection().GetService<IQueueService>().Enqueue(ResolveQueue(queue), arguments);
		}

		public void Enqueue<T>([CodeAnalysisProvider(ExecutionContext.QueueWorkerProvider)]string queue, T arguments, TimeSpan expire, TimeSpan nextVisible)
		{
			Context.Connection().GetService<IQueueService>().Enqueue(ResolveQueue(queue), arguments, expire, nextVisible);
		}

		private IQueueHandlerConfiguration ResolveQueue(string qualifier)
		{
			var config = ComponentDescriptor.Queue(new DataModelContext(Context), qualifier);

			config.Validate();

			return config.Configuration;
		}

		public void SubscriptionEvent([CodeAnalysisProvider(CodeAnalysisProviderAttribute.SubscriptionEventProvider)]string eventName, string primaryKey)
		{
			SubscriptionEvent(eventName, primaryKey, null);
		}

		public void SubscriptionEvent<T>([CodeAnalysisProvider(CodeAnalysisProviderAttribute.SubscriptionEventProvider)]string eventName, string primaryKey, T arguments)
		{
			SubscriptionEvent(eventName, primaryKey, null, arguments);
		}

		public void SubscriptionEvent([CodeAnalysisProvider(CodeAnalysisProviderAttribute.SubscriptionEventProvider)]string eventName, string primaryKey, string topic)
		{
			SubscriptionEvent<object>(eventName, primaryKey, topic, null);
		}

		public void SubscriptionEvent<T>([CodeAnalysisProvider(CodeAnalysisProviderAttribute.SubscriptionEventProvider)]string eventName, string primaryKey, string topic, T arguments)
		{
			var config = ComponentDescriptor.Subscription(new DataModelContext(Context), eventName);

			config.Validate();

			var ev = config.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, config.Element, true) == 0);

			if (ev == null)
				throw new RuntimeException($"{SR.ErrSubscriptionEventNotFound} ({config.MicroServiceName}/{config.ComponentName}/{config.Element})").WithMetrics(Context);

			Context.Connection().GetService<ISubscriptionService>().TriggerEvent(config.Configuration, config.Element, primaryKey, topic, arguments);
		}

		public Guid Event<T>([CodeAnalysisProvider(CodeAnalysisProviderAttribute.EventProvider)]string name, T e)
		{
			return Event(name, e, null);
		}

		public Guid Event([CodeAnalysisProvider(CodeAnalysisProviderAttribute.EventProvider)]string name)
		{
			return Event<object>(name, null);
		}

		public Guid Event<T>([CodeAnalysisProvider(CodeAnalysisProviderAttribute.EventProvider)]string name, T e, IEventCallback callback)
		{
			if (callback is EventCallback ec)
				ec.Attached = true;

			var config = ComponentDescriptor.Event(new DataModelContext(Context), name);

			config.Validate();

			return Context.Connection().GetService<IEventService>().Trigger(config.Configuration, callback, e);
		}

		public Guid Event([CodeAnalysisProvider(CodeAnalysisProviderAttribute.EventProvider)]string name, IEventCallback callback)
		{
			return Event<object>(name, null, callback);
		}

		public bool SubscriptionExists([CodeAnalysisProvider(CodeAnalysisProviderAttribute.SubscriptionProvider)]string subscription, string primaryKey, string topic)
		{
			var config = ComponentDescriptor.Subscription(new DataModelContext(Context), subscription);

			config.Validate();

			return Context.Connection().GetService<ISubscriptionService>().SubscriptionExists(config.Configuration, primaryKey, topic);
		}
	}
}
