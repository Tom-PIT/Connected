using System;
using System.Collections.Generic;
using TomPIT.Storage;

namespace TomPIT.Cdn.Printing
{
	internal interface IPrintingSpoolerManagementService
	{
		void Ping(Guid popReceipt);
		void Complete(Guid popReceipt);

		List<IQueueMessage> Dequeue(int count);
		Guid Insert(string mime, string printer, string content, long serialNumber, Guid identity, long copyCount = 1);
	}
}