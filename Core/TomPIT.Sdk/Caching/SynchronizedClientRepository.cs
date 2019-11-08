using TomPIT.Connectivity;

namespace TomPIT.Caching
{
	public class SynchronizedClientRepository<T, K> : SynchronizedRepository<T, K>, ITenantObject where T : class
	{
		protected SynchronizedClientRepository(ITenant tenant, string key) : base(tenant.Cache, key)
		{
			Tenant = tenant;
		}

		public ITenant Tenant { get; }
	}
}
