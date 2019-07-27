using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;

namespace TomPIT.BigData.Services
{
	internal class PartitionService : SynchronizedClientRepository<IPartition, Guid>, IPartitionService
	{
		public PartitionService(ISysConnection connection) : base(connection, "partitions")
		{
		}

		protected override void OnInitializing()
		{
			var u = Connection.CreateUrl("BigDataManagement", "QueryPartitions");
			var partitions = Connection.Get<List<Partition>>(u);

			foreach(var partition in partitions)
				Set(partition.Configuration, partition, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Connection.CreateUrl("BigDataManagement", "SelectPartition");
			var e = new JObject
			{
				{"configuration", id }
			};

			var partition = Connection.Post<Partition>(u, e);

			if (partition != null)
				Set(id, partition, TimeSpan.Zero);
		}
		public List<IPartition> Query()
		{
			return All();
		}

		public IPartition Select(IPartitionConfiguration configuration)
		{
			var r = Get(f => f.Configuration == configuration.Component);

			if (r != null)
				return r;

			var ms = Connection.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService(Connection));

			var u = Instance.Connection.CreateUrl("BigDataManagement", "InsertPartition");
			var e = new JObject
				{
					{"name", configuration.ComponentName(Connection) },
					{"configuration", configuration.Component },
					{ "status", PartitionStatus.Active.ToString() },
					{ "resourceGroup", ms.ResourceGroup }
				};

			Connection.Post(u, e);
			Refresh(configuration.Component);

			return Get(f => f.Configuration == configuration.Component);
		}

		public IPartitionFile SelectFile(long id)
		{
			throw new NotImplementedException();
		}

		public void NotifyChanged(Guid token)
		{
			Refresh(token);
		}

		public void NotifyRemoved(Guid token)
		{
			Remove(token);
		}
	}
}
