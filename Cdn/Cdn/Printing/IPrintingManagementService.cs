using System;
using System.Collections.Generic;
using TomPIT.Storage;

namespace TomPIT.Cdn.Printing
{
	internal interface IPrintingManagementService
	{
		void Ping(Guid popReceipt);
		void Complete(Guid popReceipt);
		void Error(Guid popReceipt, string error);

		List<IQueueMessage> Dequeue(int count);
	}
}
