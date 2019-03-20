using System;
using System.Collections.Generic;
using TomPIT.SysDb.Environment;

namespace TomPIT.SysDb.Messaging
{
	public interface IReliableMessagingHandler
	{
		void InsertSubscriber(IServerResourceGroup resourceGroup, ITopic topic, string connection, Guid instance);
		void DeleteSubscriber(IServerResourceGroup resourceGroup, ITopic topic, string connection);
		void UpdateSubscriber(IServerResourceGroup resourceGroup, ISubscriber subscriber, DateTime heartbeat);
		void InsertMessage(IServerResourceGroup resourceGroup, ITopic topic, Guid token, string content, DateTime expire, TimeSpan retryInterval, Guid senderInstance);
		void RemoveRecipient(IServerResourceGroup resourceGroup, ITopic topic, Guid token, string connection);
		List<IRecipient> QueryRecipients(IServerResourceGroup resourceGroup);
		List<ITopic> QueryTopics(IServerResourceGroup resourceGroup);
		ITopic SelectTopic(IServerResourceGroup resourceGroup, string name);
		void InsertTopic(IServerResourceGroup resourceGroup, string name);
		void DeleteTopic(IServerResourceGroup resourceGroup, ITopic topic);
		List<ISubscriber> QuerySubscribers(IServerResourceGroup resourceGroup);
		ISubscriber SelectSubscriber(IServerResourceGroup resourceGroup, ITopic topic, string connection);
		List<IMessage> QueryMessages(IServerResourceGroup resourceGroup);
		IMessage SelectMessage(IServerResourceGroup resourceGroup, Guid message);
		void DeleteMessage(IServerResourceGroup resourceGroup, IMessage message);
		List<IRecipient> QueryRecipients(IServerResourceGroup resourceGroup, IMessage message);
		IRecipient SelectRecipient(IServerResourceGroup resourceGroup, IMessage message, ISubscriber subscriber);
		void DeleteRecipient(IServerResourceGroup resourceGroup, IMessage message, ISubscriber subscriber);
		void DeleteRecipient(IServerResourceGroup resourceGroup, IMessage message);
		void DeleteRecipient(IServerResourceGroup resourceGroup, ITopic topic, ISubscriber subscriber);
		void UpdateRecipients(IServerResourceGroup resourceGroup, List<IRecipient> recipients);
	}
}
