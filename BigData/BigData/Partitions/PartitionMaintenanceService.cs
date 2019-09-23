using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Storage;

namespace TomPIT.BigData.Partitions
{
	internal class PartitionMaintenanceService : TenantObject, IPartitionMaintenanceService
	{
		public PartitionMaintenanceService(ITenant tenant) : base(tenant)
		{
		}

		public void Complete(Guid popReceipt, Guid partition)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "CompleteMaintenance");
			var e = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Tenant.Post(u, e);
			Tenant.GetService<IPartitionService>().SaveSchemaImage(partition);
			Tenant.GetService<IPartitionService>().NotifyChanged(partition);
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "DequeueMaintenance");
			var e = new JObject
			{
				{"count", count },
				{"nextVisible", 1800 }
			};

			return Tenant.Post<List<QueueMessage>>(u, e).ToList<IQueueMessage>();
		}

		public void Ping(Guid popReceipt)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "PingMaintenance");
			var e = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Tenant.Post(u, e);
		}
	}
}
