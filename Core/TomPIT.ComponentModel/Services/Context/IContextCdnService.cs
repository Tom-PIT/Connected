using Newtonsoft.Json.Linq;
using System;

namespace TomPIT.Services.Context
{
	public interface IContextCdnService
	{
		Guid SendMail(string from, string to, string subject, string body);
		Guid SendMail(string from, string to, string subject, string body, JArray headers, int attachmentCount);

		void CreateSubscription(string subscription, string primaryKey);
		void TriggerEvent(string subscription, string primaryKey, string eventName, JObject arguments);

		void CreateSubscription(string subscription, string primaryKey, string topic);
		void TriggerEvent(string subscription, string primaryKey, string topic, string eventName, JObject arguments);
	}
}
