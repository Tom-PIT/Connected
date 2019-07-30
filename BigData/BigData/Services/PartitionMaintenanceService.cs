using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.BigData.Services
{
	internal class PartitionMaintenanceService : ServiceBase, IPartitionMaintenanceService
	{
		public PartitionMaintenanceService(ISysConnection connection) : base(connection)
		{
		}

		public void Complete(Guid popReceipt, Guid partition)
		{
			var u = Connection.CreateUrl("BigDataManagement", "CompleteMaintenance");
			var e = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Connection.Post(u, e);
			Connection.GetService<IPartitionService>().SaveSchemaImage(partition);
			Connection.GetService<IPartitionService>().NotifyChanged(partition);
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			var u = Connection.CreateUrl("BigDataManagement", "DequeueMaintenance");
			var e = new JObject
			{
				{"count", count },
				{"nextVisible", 1800 }
			};

			return Connection.Post<List<QueueMessage>>(u, e).ToList<IQueueMessage>();
		}

		public void Ping(Guid popReceipt)
		{
			var u = Connection.CreateUrl("BigDataManagement", "PingMaintenance");
			var e = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Connection.Post(u, e);
		}
	}
}
