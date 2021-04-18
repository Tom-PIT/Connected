using System;

namespace TomPIT.SysDb.Messaging
{
	public interface IQueueMessageModifier
	{
		void Modify(DateTime nextVisible, DateTime dequeueTimestamp, int dequeueCount, Guid popReceipt);
	}
}
