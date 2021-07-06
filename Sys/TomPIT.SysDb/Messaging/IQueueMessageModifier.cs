using System;

namespace TomPIT.SysDb.Messaging
{
	public interface IQueueMessageModifier
	{
		bool Modify(DateTime nextVisible, DateTime dequeueTimestamp, int dequeueCount, Guid popReceipt);
		void Reset();
	}
}
