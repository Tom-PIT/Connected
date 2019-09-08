using Newtonsoft.Json.Linq;
using System;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Services.Context
{
	public interface IContextCdnService
	{
		Guid SendMail(string from, string to, string subject, string body);
		Guid SendMail(string from, string to, string subject, string body, JArray headers, int attachmentCount);

		string CreateMailMessage([CodeAnalysisProvider(ExecutionContext.MailTemplateProvider)]string template, string user);
		string CreateMailMessage<T>([CodeAnalysisProvider(ExecutionContext.MailTemplateProvider)]string template, string user, T arguments);

		bool SubscriptionExists([CodeAnalysisProvider(ExecutionContext.SubscriptionProvider)]string subscription, string primaryKey, string topic);
		void CreateSubscription([CodeAnalysisProvider(ExecutionContext.SubscriptionProvider)]string subscription, string primaryKey);
		void CreateSubscription([CodeAnalysisProvider(ExecutionContext.SubscriptionProvider)]string subscription, string primaryKey, string topic);
		void Enqueue<T>([CodeAnalysisProvider(ExecutionContext.QueueWorkerProvider)]string queue, T arguments);
		void Enqueue<T>([CodeAnalysisProvider(ExecutionContext.QueueWorkerProvider)]string queue, T arguments, TimeSpan expire, TimeSpan nextVisible);

		void SubscriptionEvent([CodeAnalysisProvider(ExecutionContext.SubscriptionEventProvider)]string eventName, string primaryKey);
		void SubscriptionEvent<T>([CodeAnalysisProvider(ExecutionContext.SubscriptionEventProvider)]string eventName, string primaryKey, T arguments);
		void SubscriptionEvent([CodeAnalysisProvider(ExecutionContext.SubscriptionEventProvider)]string eventName, string primaryKey, string topic);
		void SubscriptionEvent<T>([CodeAnalysisProvider(ExecutionContext.SubscriptionEventProvider)]string eventName, string primaryKey, string topic, T arguments);

		Guid Event<T>([CodeAnalysisProvider(CodeAnalysisProviderAttribute.EventProvider)]string name, T e);
		Guid Event([CodeAnalysisProvider(CodeAnalysisProviderAttribute.EventProvider)]string name);
		Guid Event<T>([CodeAnalysisProvider(CodeAnalysisProviderAttribute.EventProvider)]string name, T e, IEventCallback callback);
		Guid Event([CodeAnalysisProvider(CodeAnalysisProviderAttribute.EventProvider)]string name, IEventCallback callback);
	}
}
