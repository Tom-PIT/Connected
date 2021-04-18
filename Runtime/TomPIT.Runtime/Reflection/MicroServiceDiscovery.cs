using TomPIT.Connectivity;

namespace TomPIT.Reflection
{
	internal class MicroServiceDiscovery : TenantObject, IMicroServiceDiscovery
	{
		private IMicroServiceReferencesDiscovery _references;
		private IMicroServiceInfoDiscovery _info;

		public MicroServiceDiscovery(ITenant tenant) : base(tenant)
		{
		}

		public IMicroServiceReferencesDiscovery References => _references ??= new MicroServiceReferencesDiscovery(Tenant);

		public IMicroServiceInfoDiscovery Info => _info ??= new MicroServiceInfoDiscovery(Tenant);
	}
}
