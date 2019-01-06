using TomPIT.ComponentModel;
using TomPIT.Configuration;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Net;
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
			Shell.GetService<IConnectivityService>().ContextRegistered += OnContextRegistered;
		}

		private static void OnContextRegistered(object sender, SysContextRegisteredArgs e)
		{
			e.Context.RegisterService(typeof(ISettingManagementService), typeof(SettingManagementService));
			e.Context.RegisterService(typeof(IUserManagementService), typeof(UserManagementService));
			e.Context.RegisterService(typeof(IMembershipManagementService), typeof(MembershipManagementService));
			e.Context.RegisterService(typeof(IRoleManagementService), typeof(RoleManagementService));
			e.Context.RegisterService(typeof(ILoggingManagementService), typeof(LoggingManagementService));
			e.Context.RegisterService(typeof(IResourceGroupManagementService), typeof(ResourceGroupManagementService));
			e.Context.RegisterService(typeof(IMicroServiceManagementService), typeof(MicroServiceManagementService));
			e.Context.RegisterService(typeof(IInstanceEndpointManagementService), typeof(InstanceEndpointManagementService));
			e.Context.RegisterService(typeof(IEnvironmentUnitManagementService), typeof(EnvironmentUnitManagementService));
		}
	}
}
