using System.Collections.Generic;

namespace TomPIT.Connectivity
{
	public delegate void TenantHandler(object sender, TenantArgs e);

	public interface IConnectivityService
	{
		event TenantHandler TenantInitialized;
		event TenantHandler TenantInitialize;
		event TenantHandler TenantInitializing;

		void InsertTenant(string name, string url, string authenticationToken);

		ITenant SelectTenant(string url);
		ITenant SelectDefaultTenant();

		List<ITenantDescriptor> QueryTenants();
	}
}
