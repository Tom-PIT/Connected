using System;
using System.Collections.Generic;
using TomPIT.Storage;
using TomPIT.SysDb.Environment;

namespace TomPIT.SysDb.Messaging
{
	public enum QueueScope
	{
		System = 0,
		Content = 1
	}

	public interface IQueueHandler
	{
		void Enqueue(IServerResourceGroup resourceGroup, string queue, string message, TimeSpan expire, TimeSpan nextVisible, QueueScope scope);
		void Enqueue(IServerResourceGroup resourceGroup, string queue, IQueueContent content, TimeSpan expire, TimeSpan nextVisible, QueueScope scope);

		IQueueMessage DequeueSystem(IServerResourceGroup resourceGroup, string queue);
		IQueueMessage DequeueSystem(IServerResourceGroup resourceGroup, string queue, TimeSpan nextVisible);
		List<IQueueMessage> DequeueSystem(IServerResourceGroup resourceGroup, string queue, int count);
		List<IQueueMessage> DequeueSystem(IServerResourceGroup resourceGroup, string queue, int count, TimeSpan nextVisible);

		IQueueMessage DequeueContent(IServerResourceGroup resourceGroup);
		IQueueMessage DequeueContent(IServerResourceGroup resourceGroup, TimeSpan nextVisible);
		List<IQueueMessage> DequeueContent(IServerResourceGroup resourceGroup, int count);
		List<IQueueMessage> DequeueContent(IServerResourceGroup resourceGroup, int count, TimeSpan nextVisible);

		IQueueMessage Select(IServerResourceGroup resourceGroup, Guid popReceipt);

		void Delete(IServerResourceGroup resourceGroup, IQueueMessage message);
		void Delete(IServerResourceGroup resourceGroup, Guid popReceipt);
		void Ping(IServerResourceGroup resourceGroup, Guid popReceipt, TimeSpan nextVisible);
	}
}
