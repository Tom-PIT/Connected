using System;
using System.Collections.Generic;
using TomPIT.Storage;

namespace TomPIT.SysDb.Messaging
{
	public interface IQueueHandler
	{
		string Insert(string queue, string message, TimeSpan expire, TimeSpan nextVisible, QueueScope scope);
		string Insert(string queue, IQueueContent content, TimeSpan expire, TimeSpan nextVisible, QueueScope scope);

		//IQueueMessage DequeueSystem(string queue);
		//IQueueMessage DequeueSystem(string queue, TimeSpan nextVisible);
		//List<IQueueMessage> DequeueSystem(string queue, int count);
		//List<IQueueMessage> DequeueSystem(string queue, int count, TimeSpan nextVisible);

		//IQueueMessage DequeueContent();
		//IQueueMessage DequeueContent(TimeSpan nextVisible);
		//List<IQueueMessage> DequeueContent(int count);
		//List<IQueueMessage> DequeueContent(int count, TimeSpan nextVisible);

		IQueueMessage Select(string id);
		List<IQueueMessage> Query();

		void Delete(IQueueMessage message);
		//void Delete(Guid popReceipt);
		//void Ping(Guid popReceipt, TimeSpan nextVisible);

		void Update(List<IQueueMessage> messages);
	}
}
