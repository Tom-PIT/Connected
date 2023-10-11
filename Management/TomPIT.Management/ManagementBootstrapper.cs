using TomPIT.Connectivity;
using TomPIT.Management.BigData;
using TomPIT.Management.ComponentModel;
using TomPIT.Management.Configuration;
using TomPIT.Management.Diagnostics;
using TomPIT.Management.Environment;
using TomPIT.Management.Globalization;
using TomPIT.Management.Security;

namespace TomPIT.Management
{
	public static class ManagementBootstrapper
	{
		public static void Initialize(ITenant tenant)
		{
			tenant.RegisterService(typeof(ISettingManagementService), typeof(SettingManagementService));
			tenant.RegisterService(typeof(IUserManagementService), typeof(UserManagementService));
			tenant.RegisterService(typeof(IMembershipManagementService), typeof(MembershipManagementService));
			tenant.RegisterService(typeof(IRoleManagementService), typeof(RoleManagementService));
			tenant.RegisterService(typeof(ILoggingManagementService), typeof(LoggingManagementService));
			tenant.RegisterService(typeof(IResourceGroupManagementService), typeof(ResourceGroupManagementService));
			tenant.RegisterService(typeof(IMicroServiceManagementService), typeof(MicroServiceManagementService));
			tenant.RegisterService(typeof(IInstanceEndpointManagementService), typeof(InstanceEndpointManagementService));
			tenant.RegisterService(typeof(IAuthenticationTokenManagementService), typeof(AuthenticationTokenManagementService));
			tenant.RegisterService(typeof(IMetricManagementService), typeof(MetricManagementService));
			tenant.RegisterService(typeof(IBigDataManagementService), typeof(BigDataManagementService));
			tenant.RegisterService(typeof(IGlobalizationManagementService), typeof(GlobalizationManagementService));
		}
	}
}
