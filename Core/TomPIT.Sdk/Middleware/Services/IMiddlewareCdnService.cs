using System;
using Newtonsoft.Json.Linq;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareCdnService
	{
		string DataHubServer { get; }
		Guid SendMail(string from, string to, string subject, string body);
		Guid SendMail(string from, string to, string subject, string body, JArray headers, int attachmentCount);

		string CreateMailMessage([CAP(CAP.MailTemplateProvider)]string template, string user);
		string CreateMailMessage<T>([CAP(CAP.MailTemplateProvider)]string template, string user, T arguments);

		bool SubscriptionExists([CAP(CAP.SubscriptionProvider)]string subscription, string primaryKey, string topic);
		void CreateSubscription([CAP(CAP.SubscriptionProvider)]string subscription, string primaryKey);
		void CreateSubscription([CAP(CAP.SubscriptionProvider)]string subscription, string primaryKey, string topic);
		void Enqueue<T>([CIP(CIP.QueueWorkersProvider)]string queue, T arguments);
		void Enqueue<T>([CIP(CIP.QueueWorkersProvider)]string queue, T arguments, TimeSpan expire, TimeSpan nextVisible);

		void SubscriptionEvent([CAP(CAP.SubscriptionEventProvider)]string eventName, string primaryKey);
		void SubscriptionEvent<T>([CAP(CAP.SubscriptionEventProvider)]string eventName, string primaryKey, T arguments);
		void SubscriptionEvent([CAP(CAP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic);
		void SubscriptionEvent<T>([CAP(CAP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic, T arguments);

		Guid DistributedEvent<T>([CIP(CIP.DistributedEventProvider)]string name, T e);
		Guid DistributedEvent([CIP(CIP.DistributedEventProvider)]string name);
		Guid DistributedEvent<T>([CIP(CIP.DistributedEventProvider)]string name, T e, IMiddlewareCallback callback);
		Guid DistributedEvent([CIP(CIP.DistributedEventProvider)]string name, IMiddlewareCallback callback);

		void Notify<T>([CIP(CIP.DataHubEndpointProvider)]string dataHubEndpoint, T e);
	}
}
