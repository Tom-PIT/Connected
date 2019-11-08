using System;
using System.Collections.Generic;
using TomPIT.Storage;

namespace TomPIT.Development.Analysis
{
	internal interface IAutoFixService
	{
		List<IQueueMessage> Dequeue(int count);
		void Ping(Guid popReceipt);
		void Complete(Guid popReceipt);
	}
}
