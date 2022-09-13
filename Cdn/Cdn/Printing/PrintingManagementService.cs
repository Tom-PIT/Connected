using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Printing
{
	internal class PrintingManagementService : IPrintingManagementService
	{
		public void Complete(Guid popReceipt)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("PrintingManagement", "Complete");
			var e = new JObject
			{
				{ "popReceipt", popReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		public List<IPrintQueueMessage> Dequeue(int count)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("PrintingManagement", "Dequeue");
			var e = new JObject
			{
				{ "count", count }
			};

			return MiddlewareDescriptor.Current.Tenant.Post<List<PrintQueueMessage>>(u, e).ToList<IPrintQueueMessage>();
		}

		public void Error(Guid popReceipt, string error)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("PrintingManagement", "Error");
			var e = new JObject
			{
				{ "popReceipt", popReceipt },
				{ "error", error }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		public void Ping(Guid popReceipt)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("PrintingManagement", "Ping");
			var e = new JObject
			{
				{ "popReceipt", popReceipt },
				{ "nextVisible", TimeSpan.FromMinutes(4) }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}
	}
}
