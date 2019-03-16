using Newtonsoft.Json.Linq;
using System;
using TomPIT.Annotations;

namespace TomPIT.Services.Context
{
	public interface IContextCdnService
	{
		Guid SendMail(string from, string to, string subject, string body);
		Guid SendMail(string from, string to, string subject, string body, JArray headers, int attachmentCount);

		string CreateMailMessage([CodeAnalysisProvider(ExecutionContext.MailTemplateProvider)]string template, string user);
		string CreateMailMessage([CodeAnalysisProvider(ExecutionContext.MailTemplateProvider)]string template, string user, JObject arguments);

		void CreateSubscription([CodeAnalysisProvider(ExecutionContext.SubscriptionProvider)]string subscription, string primaryKey);
		void TriggerEvent([CodeAnalysisProvider(ExecutionContext.SubscriptionEventProvider)]string eventName, string primaryKey);
		void TriggerEvent([CodeAnalysisProvider(ExecutionContext.SubscriptionEventProvider)]string eventName, string primaryKey, JObject arguments);

		void CreateSubscription([CodeAnalysisProvider(ExecutionContext.SubscriptionProvider)]string subscription, string primaryKey, string topic);
		void TriggerEvent([CodeAnalysisProvider(ExecutionContext.SubscriptionEventProvider)]string eventName, string primaryKey, string topic);
		void TriggerEvent([CodeAnalysisProvider(ExecutionContext.SubscriptionEventProvider)]string eventName, string primaryKey, string topic, JObject arguments);
	}
}
