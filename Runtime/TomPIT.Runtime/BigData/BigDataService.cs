using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Reflection;

namespace TomPIT.BigData
{
	internal class BigDataService : TenantObject, IBigDataService
	{
		public BigDataService(ITenant tenant) : base(tenant)
		{
		}

		public void Update<T>(IPartitionConfiguration partition, T items)
		{
			if (items == null)
				return;

			var url = Tenant.GetService<IInstanceEndpointService>().Url(InstanceType.BigData, InstanceVerbs.Post);

			if (string.IsNullOrWhiteSpace(url))
				throw new RuntimeException($"{SR.ErrNoServer} ({InstanceType.BigData}, {InstanceVerbs.Post})");

			var ms = Tenant.GetService<IMicroServiceService>().Select(partition.MicroService());

			var u = $"{url}/data/{ms.Name}/{partition.ComponentName()}";
			object e = items;

			if (!e.GetType().IsCollection())
				e = new List<object> { items };

			Tenant.Post(u, e);
		}
	}
}
