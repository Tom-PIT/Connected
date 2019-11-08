using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Cdn;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Diagostics;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Messaging;
using TomPIT.Runtime;
using TomPIT.Serialization;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareCdnService : MiddlewareObject, IMiddlewareCdnService
	{
		private string _dataHubServer = null;
		public MiddlewareCdnService(IMiddlewareContext context) : base(context)
		{
		}

		public string DataHubServer
		{
			get
			{
				if (_dataHubServer == null)
				{
					_dataHubServer = Context.Services.Routing.GetServer(InstanceType.Cdn, InstanceVerbs.All);

					if (_dataHubServer == null)
						throw new RuntimeException(SR.ErrNoCdnServer);

					_dataHubServer = $"{_dataHubServer}/dataHub";
				}

				return _dataHubServer;
			}
		}

		public Guid SendMail(string from, string to, string subject, string body)
		{
			return SendMail(from, to, subject, body, null, 0);
		}

		public Guid SendMail(string from, string to, string subject, string body, JArray headers, int attachmentCount)
		{
			return Context.Tenant.GetService<IMailService>().Enqueue(from, to, subject, body, headers, attachmentCount, MailFormat.Html, DateTime.UtcNow, DateTime.UtcNow.AddHours(24));
		}

		public string CreateMailMessage(string template, string user)
		{
			return CreateMailMessage<object>(template, user, null);
		}

		public string CreateMailMessage<T>(string template, string user, T arguments)
		{
			var descriptor = ComponentDescriptor.MailTemplate(Context, template);

			descriptor.Validate();

			var url = Context.Tenant.GetService<IRuntimeService>().Type == InstanceType.Application
				? Context.Services.Routing.RootUrl
				: Context.Tenant.GetService<IInstanceEndpointService>().Url(InstanceType.Application, InstanceVerbs.Post);

			if (string.IsNullOrWhiteSpace(url))
				throw new RuntimeException(SR.ErrNoAppServer).WithMetrics(Context);

			url = $"{url}/sys/mail-template/{descriptor.Component.Token}";

			var e = new JObject();

			if (!string.IsNullOrWhiteSpace(user))
				e.Add("user", user);

			if (arguments != null)
				e.Add("arguments", Serializer.Serialize(arguments));

			return Context.Tenant.Post<string>(url, e, new Connectivity.HttpRequestArgs
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
			var config = ComponentDescriptor.Subscription(Context, subscription);

			config.Validate();

			Context.Tenant.GetService<ISubscriptionService>().CreateSubscription(config.Configuration, primaryKey, topic);
		}

		public void Enqueue<T>([CIP(CIP.QueueWorkersProvider)]string queue, T arguments)
		{
			Context.Tenant.GetService<IQueueService>().Enqueue(ResolveQueue(queue), arguments);
		}

		public void Enqueue<T>([CIP(CIP.QueueWorkersProvider)]string queue, T arguments, TimeSpan expire, TimeSpan nextVisible)
		{
			Context.Tenant.GetService<IQueueService>().Enqueue(ResolveQueue(queue), arguments, expire, nextVisible);
		}

		private IQueueWorker ResolveQueue(string qualifier)
		{
			var config = ComponentDescriptor.Queue(Context, qualifier);

			config.Validate();

			return config.Configuration.Workers.FirstOrDefault(f => string.Compare(f.Name, config.Element, true) == 0);
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
			var config = ComponentDescriptor.Subscription(Context, eventName);

			config.Validate();

			var ev = config.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, config.Element, true) == 0);

			if (ev == null)
				throw new RuntimeException($"{SR.ErrSubscriptionEventNotFound} ({config.MicroServiceName}/{config.ComponentName}/{config.Element})").WithMetrics(Context);

			Context.Tenant.GetService<ISubscriptionService>().TriggerEvent(config.Configuration, config.Element, primaryKey, topic, arguments);
		}

		public Guid DistributedEvent<T>([CIP(CIP.DistributedEventProvider)]string name, T e)
		{
			return DistributedEvent(name, e, null);
		}

		public Guid DistributedEvent([CIP(CIP.DistributedEventProvider)]string name)
		{
			return DistributedEvent<object>(name, null);
		}

		public Guid DistributedEvent<T>([CIP(CIP.DistributedEventProvider)]string name, T e, IMiddlewareCallback callback)
		{
			if (callback is MiddlewareCallback ec)
				ec.Attached = true;

			var config = ComponentDescriptor.DistributedEvent(Context, name);

			config.Validate();

			var ev = config.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, config.Element, true) == 0);

			if (ev == null)
				throw new RuntimeException($"{SR.ErrDistributedEventNotFound} ({name})");

			return Context.Tenant.GetService<IEventService>().Trigger(ev, callback, e);
		}

		public Guid DistributedEvent([CIP(CIP.DistributedEventProvider)]string name, IMiddlewareCallback callback)
		{
			return DistributedEvent<object>(name, null, callback);
		}

		public bool SubscriptionExists([CodeAnalysisProvider(CodeAnalysisProviderAttribute.SubscriptionProvider)]string subscription, string primaryKey, string topic)
		{
			var config = ComponentDescriptor.Subscription(Context, subscription);

			config.Validate();

			return Context.Tenant.GetService<ISubscriptionService>().SubscriptionExists(config.Configuration, primaryKey, topic);
		}

		public void Notify<T>([CIP(CIP.DataHubEndpointProvider)] string dataHubEndpoint, T e)
		{
			var descriptor = ComponentDescriptor.DataHub(Context, dataHubEndpoint);

			descriptor.Validate();

			var endpoint = Context.Services.Routing.GetServer(InstanceType.Cdn, InstanceVerbs.Post);

			if (endpoint == null)
				throw new RuntimeException(SR.ErrNoCdnServer);

			var url = $"{endpoint}/data";
			var args = new JObject
			{
				{"endpoint",  $"{descriptor.MicroService.Name}/{descriptor.Component.Name}/{descriptor.Element}"}
			};

			if (e != null)
				args.Add("arguments", Serializer.Deserialize<JObject>(Serializer.Serialize(e)));

			Context.Tenant.Post(url, args);
		}
	}
}
