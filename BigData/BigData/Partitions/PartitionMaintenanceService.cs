using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
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
			Instance.SysProxy.Management.BigData.CompleteMaintenance(popReceipt);
			Tenant.GetService<IPartitionService>().SaveSchemaImage(partition);
			Tenant.GetService<IPartitionService>().NotifyChanged(partition);
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			return Instance.SysProxy.Management.BigData.DequeueMaintenance(count, 1800).ToList();
		}

		public void Ping(Guid popReceipt)
		{
			Instance.SysProxy.Management.BigData.PingMaintenance(popReceipt, 60);
		}
	}
}
