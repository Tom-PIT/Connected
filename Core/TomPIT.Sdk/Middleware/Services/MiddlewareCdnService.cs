using System;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareCdnService : MiddlewareObject, IMiddlewareCdnService
	{
		private IMiddlewareEmail _email = null;
		private IMiddlewareSubscriptions _subscriptions = null;
		private IMiddlewareDataHub _dataHub = null;
		private IMiddlewareEvents _events = null;
		private IMiddlewareQueue _queue = null;
		private IMiddlewarePrinting _printing = null;
		public MiddlewareCdnService(IMiddlewareContext context) : base(context)
		{
		}

		public string DataHubServer => DataHub.Server;

		public IMiddlewareEmail Mail
		{
			get
			{
				if (_email == null)
					_email = new MiddlewareEmail();

				return _email;
			}
		}

		public IMiddlewareSubscriptions Subscriptions
		{
			get
			{
				if (_subscriptions == null)
					_subscriptions = new MiddlewareSubscriptions();

				return _subscriptions;
			}
		}

		public IMiddlewareDataHub DataHub
		{
			get
			{
				if (_dataHub == null)
					_dataHub = new MiddlewareDataHub();

				return _dataHub;
			}
		}

		public IMiddlewareEvents Events
		{
			get
			{
				if (_events == null)
					_events = new MiddlewareEvents();

				return _events;
			}
		}

		public IMiddlewareQueue Queue
		{
			get
			{
				if (_queue == null)
					_queue = new MiddlewareQueue();

				return _queue;
			}
		}

		public IMiddlewarePrinting Printing
		{
			get
			{
				if (_printing == null)
					_printing = new MiddlewarePrinting();

				return _printing;
			}
		}

		public Guid SendMail(string from, string to, string subject, string body)
		{
			return SendMail(from, to, subject, body, null, 0);
		}

		public Guid SendMail(string from, string to, string subject, string body, JArray headers, int attachmentCount)
		{
			return Mail.Send(from, to, subject, body, headers, attachmentCount);
		}

		public string CreateMailMessage(string template, string user)
		{
			return CreateMailMessage<object>(template, user, null);
		}

		public string CreateMailMessage<T>(string template, string user, T arguments)
		{
			return Mail.Create(template, user, arguments);
		}

		public void CreateSubscription(string subscription, string primaryKey)
		{
			CreateSubscription(subscription, primaryKey, null);
		}

		public void CreateSubscription(string subscription, string primaryKey, string topic)
		{
			Subscriptions.Create(subscription, primaryKey, topic);
		}

		public void Enqueue<T>([CIP(CIP.QueueWorkersProvider)]string queue, T arguments)
		{
			Queue.Enqueue(queue, arguments);
		}

		public void Enqueue<T>([CIP(CIP.QueueWorkersProvider)]string queue, T arguments, TimeSpan expire, TimeSpan nextVisible)
		{
			Queue.Enqueue(queue, arguments, expire, nextVisible);
		}

		public void SubscriptionEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey)
		{
			SubscriptionEvent(eventName, primaryKey, null);
		}

		public void SubscriptionEvent<T>([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, T arguments)
		{
			SubscriptionEvent(eventName, primaryKey, null, arguments);
		}

		public void SubscriptionEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic)
		{
			SubscriptionEvent<object>(eventName, primaryKey, topic, null);
		}

		public void SubscriptionEvent<T>([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic, T arguments)
		{
			Subscriptions.TriggerEvent(eventName, primaryKey, topic, arguments);
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
			return Events.TriggerEvent(name, e, callback);
		}

		public Guid DistributedEvent([CIP(CIP.DistributedEventProvider)]string name, IMiddlewareCallback callback)
		{
			return DistributedEvent<object>(name, null, callback);
		}

		public bool SubscriptionExists([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string topic)
		{
			return Subscriptions.Exists(subscription, primaryKey, topic);
		}

		public void Notify<T>([CIP(CIP.DataHubEndpointProvider)] string dataHubEndpoint, T e)
		{
			DataHub.Notify(dataHubEndpoint, e);
		}

		public Guid PrintReport(string report, IPrinter printer)
		{
			return PrintReport(report, printer, null);
		}

		public Guid PrintReport(string report, IPrinter printer, object arguments)
		{
			return PrintReport(report, printer, arguments, null);
		}

		public Guid PrintReport(string report, IPrinter printer, object arguments, string provider)
		{
			return Printing.PrintReport(report, printer, arguments, provider);
		}

		public IPrintJob SelectPrintJob(Guid job)
		{
			return Context.Tenant.GetService<IPrintingService>().Select(job);
		}
	}
}
