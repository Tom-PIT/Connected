using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Exceptions;

namespace TomPIT.BigData
{
	internal class BigDataService : TenantObject, IBigDataService
	{
		public BigDataService(ITenant tenant) : base(tenant)
		{
		}

		public void Add<T>(IPartitionConfiguration partition, List<T> items)
		{
			var url = Tenant.GetService<IInstanceEndpointService>().Url(InstanceType.BigData, InstanceVerbs.Post);

			if (string.IsNullOrWhiteSpace(url))
				throw new RuntimeException($"{SR.ErrNoServer} ({InstanceType.BigData}, {InstanceVerbs.Post})");

			var ms = Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)partition).MicroService());

			var u = $"{url}/data/{ms.Name}/{partition.ComponentName()}";

			Tenant.Post(u, items);
		}
	}
}
