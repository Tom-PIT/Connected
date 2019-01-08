using TomPIT.ComponentModel;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Security;

namespace TomPIT
{
	public static class ManagementBootstrapper
	{
		public static void Run()
		{
			RegisterServices();
		}

		private static void RegisterServices()
		{
			Shell.GetService<IConnectivityService>().ConnectionRegistered += OnConnectionRegistered;
		}

		private static void OnConnectionRegistered(object sender, SysConnectionRegisteredArgs e)
		{
			e.Connection.RegisterService(typeof(ISettingManagementService), typeof(SettingManagementService));
			e.Connection.RegisterService(typeof(IUserManagementService), typeof(UserManagementService));
			e.Connection.RegisterService(typeof(IMembershipManagementService), typeof(MembershipManagementService));
			e.Connection.RegisterService(typeof(IRoleManagementService), typeof(RoleManagementService));
			e.Connection.RegisterService(typeof(ILoggingManagementService), typeof(LoggingManagementService));
			e.Connection.RegisterService(typeof(IResourceGroupManagementService), typeof(ResourceGroupManagementService));
			e.Connection.RegisterService(typeof(IMicroServiceManagementService), typeof(MicroServiceManagementService));
			e.Connection.RegisterService(typeof(IInstanceEndpointManagementService), typeof(InstanceEndpointManagementService));
			e.Connection.RegisterService(typeof(IEnvironmentUnitManagementService), typeof(EnvironmentUnitManagementService));
		}
	}
}
