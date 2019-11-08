using System;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

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
				var u = Tenant.CreateUrl("MicroService", "SelectMeta")
					.AddParameter("microService", microService);

				return Tenant.Get<string>(u);
			});
		}
	}
}
