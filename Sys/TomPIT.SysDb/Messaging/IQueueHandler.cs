using System;
using System.Collections.Generic;
using TomPIT.Storage;

namespace TomPIT.SysDb.Messaging
{
	public interface IQueueHandler
	{
		string Insert(string queue, string message, string bufferKey, TimeSpan expire, TimeSpan nextVisible, QueueScope scope);
		string Insert(string queue, IQueueContent content, string bufferKey, TimeSpan expire, TimeSpan nextVisible, QueueScope scope);

		IQueueMessage Select(string id);
		List<IQueueMessage> Query();

		void Delete(IQueueMessage message);
		void Update(List<IQueueMessage> messages);
	}
}
