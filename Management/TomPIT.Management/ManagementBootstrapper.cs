using TomPIT.Connectivity;
using TomPIT.Management.BigData;
using TomPIT.Management.ComponentModel;
using TomPIT.Management.Configuration;
using TomPIT.Management.Deployment;
using TomPIT.Management.Diagnostics;
using TomPIT.Management.Environment;
using TomPIT.Management.Globalization;
using TomPIT.Management.Security;

namespace TomPIT.Management
{
	public static class ManagementBootstrapper
	{
		public static void Run()
		{
			RegisterServices();
		}

		private static void RegisterServices()
		{
			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
		}

		private static void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(ISettingManagementService), typeof(SettingManagementService));
			e.Tenant.RegisterService(typeof(IUserManagementService), typeof(UserManagementService));
			e.Tenant.RegisterService(typeof(IMembershipManagementService), typeof(MembershipManagementService));
			e.Tenant.RegisterService(typeof(IRoleManagementService), typeof(RoleManagementService));
			e.Tenant.RegisterService(typeof(ILoggingManagementService), typeof(LoggingManagementService));
			e.Tenant.RegisterService(typeof(IResourceGroupManagementService), typeof(ResourceGroupManagementService));
			e.Tenant.RegisterService(typeof(IMicroServiceManagementService), typeof(MicroServiceManagementService));
			e.Tenant.RegisterService(typeof(IInstanceEndpointManagementService), typeof(InstanceEndpointManagementService));
			e.Tenant.RegisterService(typeof(IAuthenticationTokenManagementService), typeof(AuthenticationTokenManagementService));
			e.Tenant.RegisterService(typeof(IDeploymentService), typeof(DeploymentService));
			e.Tenant.RegisterService(typeof(IMetricManagementService), typeof(MetricManagementService));
			e.Tenant.RegisterService(typeof(IBigDataManagementService), typeof(BigDataManagementService));
			e.Tenant.RegisterService(typeof(IGlobalizationManagementService), typeof(GlobalizationManagementService));
		}
	}
}
