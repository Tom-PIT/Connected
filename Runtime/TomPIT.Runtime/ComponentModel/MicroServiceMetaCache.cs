using System;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	internal class MicroServiceMetaCache : ClientRepository<string, Guid>
	{
		public MicroServiceMetaCache(ISysConnection connection) : base(connection, "microservicemeta")
		{
		}

		public string Select(Guid microService)
		{
			return Get(microService, f =>
			{
				var u = Connection.CreateUrl("MicroService", "SelectMeta")
					.AddParameter("microService", microService);

				return Connection.Get<string>(u);
			});
		}
	}
}
