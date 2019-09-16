using System;
using Newtonsoft.Json.Linq;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareCdnService
	{
		Guid SendMail(string from, string to, string subject, string body);
		Guid SendMail(string from, string to, string subject, string body, JArray headers, int attachmentCount);

		string CreateMailMessage([CAP(CAP.MailTemplateProvider)]string template, string user);
		string CreateMailMessage<T>([CAP(CAP.MailTemplateProvider)]string template, string user, T arguments);

		bool SubscriptionExists([CAP(CAP.SubscriptionProvider)]string subscription, string primaryKey, string topic);
		void CreateSubscription([CAP(CAP.SubscriptionProvider)]string subscription, string primaryKey);
		void CreateSubscription([CAP(CAP.SubscriptionProvider)]string subscription, string primaryKey, string topic);
		void Enqueue<T>([CAP(CAP.QueueWorkerProvider)]string queue, T arguments);
		void Enqueue<T>([CAP(CAP.QueueWorkerProvider)]string queue, T arguments, TimeSpan expire, TimeSpan nextVisible);

		void SubscriptionEvent([CAP(CAP.SubscriptionEventProvider)]string eventName, string primaryKey);
		void SubscriptionEvent<T>([CAP(CAP.SubscriptionEventProvider)]string eventName, string primaryKey, T arguments);
		void SubscriptionEvent([CAP(CAP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic);
		void SubscriptionEvent<T>([CAP(CAP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic, T arguments);

		Guid Event<T>([CAP(CAP.EventProvider)]string name, T e);
		Guid Event([CAP(CAP.EventProvider)]string name);
		Guid Event<T>([CAP(CAP.EventProvider)]string name, T e, IMiddlewareCallback callback);
		Guid Event([CAP(CAP.EventProvider)]string name, IMiddlewareCallback callback);
	}
}
