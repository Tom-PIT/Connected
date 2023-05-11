using System;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	internal class MicroServiceMetaCache : ClientRepository<string, Guid>
	{
		public MicroServiceMetaCache(ITenant tenant) : base(tenant, "microservicemeta")
		{
		}

		public string Select(Guid microService)
		{
			return Get(microService, f =>
			{
				return Instance.SysProxy.MicroServices.SelectMeta(microService);
			});
		}
	}
}
