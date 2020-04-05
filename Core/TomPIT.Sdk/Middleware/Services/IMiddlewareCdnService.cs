using System;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareCdnService
	{
		[Obsolete]
		string DataHubServer { get; }
		IMiddlewareEmail Mail { get; }
		IMiddlewareSubscriptions Subscriptions { get; }
		IMiddlewareEvents Events { get; }
		IMiddlewareDataHub DataHub { get; }
		IMiddlewarePrinting Printing { get; }
		IMiddlewareQueue Queue { get; }
		[Obsolete]
		Guid SendMail(string from, string to, string subject, string body);
		[Obsolete]
		Guid SendMail(string from, string to, string subject, string body, JArray headers, int attachmentCount);
		[Obsolete]
		string CreateMailMessage([CAP(CAP.MailTemplateProvider)]string template, string user);
		[Obsolete]
		string CreateMailMessage<T>([CAP(CAP.MailTemplateProvider)]string template, string user, T arguments);
		[Obsolete]
		bool SubscriptionExists([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string topic);
		[Obsolete]
		void CreateSubscription([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey);
		[Obsolete]
		void CreateSubscription([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string topic);
		[Obsolete]
		void Enqueue<T>([CIP(CIP.QueueWorkersProvider)]string queue, T arguments);
		[Obsolete]
		void Enqueue<T>([CIP(CIP.QueueWorkersProvider)]string queue, T arguments, TimeSpan expire, TimeSpan nextVisible);
		[Obsolete]
		void SubscriptionEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey);
		[Obsolete]
		void SubscriptionEvent<T>([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, T arguments);
		[Obsolete]
		void SubscriptionEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic);
		[Obsolete]
		void SubscriptionEvent<T>([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic, T arguments);
		[Obsolete]
		Guid DistributedEvent<T>([CIP(CIP.DistributedEventProvider)]string name, T e);
		[Obsolete]
		Guid DistributedEvent([CIP(CIP.DistributedEventProvider)]string name);
		[Obsolete]
		Guid DistributedEvent<T>([CIP(CIP.DistributedEventProvider)]string name, T e, IMiddlewareCallback callback);
		[Obsolete]
		Guid DistributedEvent([CIP(CIP.DistributedEventProvider)]string name, IMiddlewareCallback callback);
		[Obsolete]
		void Notify<T>([CIP(CIP.DataHubEndpointProvider)]string dataHubEndpoint, T e);
		[Obsolete]
		Guid PrintReport([CIP(CIP.ReportProvider)]string report, IPrinter printer);
		[Obsolete]
		Guid PrintReport([CIP(CIP.ReportProvider)]string report, IPrinter printer, object arguments);
		[Obsolete]
		Guid PrintReport([CIP(CIP.ReportProvider)]string report, IPrinter printer, object arguments, string provider);
		[Obsolete]
		IPrintJob SelectPrintJob(Guid job);
	}
}
