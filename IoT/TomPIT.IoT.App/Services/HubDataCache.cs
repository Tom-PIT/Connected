using Newtonsoft.Json.Linq;
using System;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.IoT.Services
{
	internal class HubDataCache : SynchronizedClientRepository<JObject, Guid>
	{
		public HubDataCache(ISysConnection connection) : base(connection, "hubdata")
		{
		}

		protected override void OnInitializing()
		{
			base.OnInitializing();
		}
	}
}
