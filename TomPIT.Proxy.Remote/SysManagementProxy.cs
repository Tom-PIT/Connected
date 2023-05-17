using TomPIT.Proxy.Management;
using TomPIT.Proxy.Remote.Management;

namespace TomPIT.Proxy.Remote
{
	internal class SysManagementProxy : ISysManagementProxy
	{
		public SysManagementProxy()
		{
			Roles = new RoleManagementController();
			Settings = new SettingManagementController();
			BigData = new BigDataManagementController();
			Users = new UserManagementController();
			MicroServices = new MicroServiceManagementController();
		}

		public IRoleManagementController Roles { get; }
		public ISettingManagementController Settings { get; }
		public IBigDataManagementController BigData { get; }
		public IUserManagementController Users { get; }
		public IMicroServiceManagementController MicroServices { get; }
	}
}
