using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Services;

namespace TomPIT.BigData
{
	internal class BigDataService : ServiceBase, IBigDataService
	{
		public BigDataService(ISysConnection connection) : base(connection)
		{
		}

		public void Add<T>(IPartitionConfiguration partition, List<T> items)
		{
			var url = Connection.GetService<IInstanceEndpointService>().Url(InstanceType.BigData, InstanceVerbs.Post);

			if (string.IsNullOrWhiteSpace(url))
				throw new RuntimeException($"{SR.ErrNoServer} ({InstanceType.BigData}, {InstanceVerbs.Post})");

			var ms = Connection.GetService<IMicroServiceService>().Select(((IConfiguration)partition).MicroService(Connection));

			var u = $"{url}/data/{ms.Name}/{partition.ComponentName(Connection)}";

			Connection.Post(u, items);
		}
	}
}
