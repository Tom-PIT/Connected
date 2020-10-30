using System;
using System.Collections.Generic;
using TomPIT.SysDb.Environment;

namespace TomPIT.SysDb.Messaging
{
	public interface IReliableMessagingHandler
	{
		void InsertSubscriber(ITopic topic, string connection, Guid instance);
		void DeleteSubscriber(ITopic topic, string connection);
		void UpdateSubscriber(ISubscriber subscriber, DateTime heartbeat);
		void InsertMessage(ITopic topic, Guid token, string content, DateTime expire, TimeSpan retryInterval, Guid senderInstance);
		void RemoveRecipient(ITopic topic, Guid token, string connection);
		List<IRecipient> QueryRecipients();
		List<ITopic> QueryTopics();
		ITopic SelectTopic(string name);
		void InsertTopic(IServerResourceGroup resourceGroup, string name);
		void DeleteTopic(ITopic topic);
		List<ISubscriber> QuerySubscribers();
		ISubscriber SelectSubscriber(ITopic topic, string connection);
		List<IMessage> QueryMessages();
		IMessage SelectMessage(Guid message);
		void DeleteMessage(IMessage message);

		void Clean(List<IMessage> messages, List<IRecipient> recipients);
		List<IRecipient> QueryRecipients(IMessage message);
		IRecipient SelectRecipient(IMessage message, ISubscriber subscriber);
		void DeleteRecipient(IMessage message, ISubscriber subscriber);
		void DeleteRecipient(IMessage message);
		void DeleteRecipient(ITopic topic, ISubscriber subscriber);
		void UpdateRecipients(List<IRecipient> recipients);
	}
}
