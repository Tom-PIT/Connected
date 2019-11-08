using TomPIT.Connectivity;

namespace TomPIT.Caching
{
	public abstract class ClientRepository<T, K> : CacheRepository<T, K>, ITenantObject where T : class
	{
		protected ClientRepository(ITenant tenant, string key) : base(tenant.Cache, key)
		{
			Tenant = tenant;
		}

		public ITenant Tenant { get; }
	}
}
