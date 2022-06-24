using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Storage;

namespace TomPIT.Cdn.Printing
{
	internal class PrintingSpoolerManagementService : IPrintingSpoolerManagementService
	{
		public void Complete(Guid popReceipt)
		{
			MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("CompleteSpooler"), new
			{
				popReceipt
			});
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			return MiddlewareDescriptor.Current.Tenant.Post<List<QueueMessage>>(CreateUrl("DequeueSpooler"), new
			{
				count
			}).ToList<IQueueMessage>();
		}

		public void Ping(Guid popReceipt)
		{
			MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("PingSpooler"), new
			{
				popReceipt,
				NextVisible = TimeSpan.FromSeconds(5)
			});
		}

		public Guid Insert(string mime, string printer, string content, long serialNumber, Guid identity, long copyCount = 1)
		{
			return MiddlewareDescriptor.Current.Tenant.Post<Guid>(CreateUrl("InsertSpooler"), new
			{
				mime,
				printer,
				content,
				serialNumber,
				identity,
				copyCount
			});
		}

		private ServerUrl CreateUrl(string method)
		{
			return MiddlewareDescriptor.Current.Tenant.CreateUrl("PrintingManagement", method);
		}
	}
}
